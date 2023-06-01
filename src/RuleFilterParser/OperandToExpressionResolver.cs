using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using RuleFilterParser.Exceptions;

namespace RuleFilterParser;

public static class OperandToExpressionResolver
{

    public static object ConvertValue(Type type, object value)
    {

        try
        {
            value = type switch
            {
                Type when type == typeof(bool) && value is string strVal => strVal == "1",
                Type when type == typeof(int) && value is string strVal => (int)double.Parse(strVal, CultureInfo.InvariantCulture),
                Type when type == typeof(decimal) && value is string strVal => decimal.Parse(strVal, CultureInfo.InvariantCulture),
                Type when type == typeof(DateTime) && value is string strVal && DateTime.TryParse(strVal, out var dateTime) => dateTime,
                Type when type == typeof(int) && value is ValueTuple<string, string> tuple => new ValueTuple<int, int>(
                    (int)ConvertValue(type, tuple.Item1), (int)ConvertValue(type, tuple.Item2)),
                Type when type == typeof(double) && value is ValueTuple<string, string> tuple => new ValueTuple<double, double>(
                    (double)ConvertValue(type, tuple.Item1), (double)ConvertValue(type, tuple.Item2)),
                Type when type == typeof(DateTime) && value is ValueTuple<string, string> tuple => new ValueTuple<DateTime, DateTime>(
                    (DateTime)ConvertValue(type, tuple.Item1), (DateTime)ConvertValue(type, tuple.Item2)),
                Type when type == typeof(int) && value is not int => int.Parse(value.ToString()),
                _ => value
            };


            value = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
        }

        return value;
    }

    public static Expression GetExpressionForRule(Expression property, string operand, object value)
    {

        Type propertyType = property.Type;
        if (Nullable.GetUnderlyingType(propertyType) != null)
        {
            propertyType = Nullable.GetUnderlyingType(propertyType);
            property = Expression.Convert(property, propertyType);
        }

        switch (operand)
        {
            case "_eq":
                return Expression.Equal(property, Expression.Constant(ConvertValue(property.Type, value)));
            case "_neq":
                return Expression.NotEqual(property, Expression.Constant(ConvertValue(property.Type, value)));
            case
                "_contains" or "_ncontains" or
                "_starts_with" or "_nstarts_with" or
                "_ends_with" or "_nends_with":
                {
                    var propertyAsString = ConvertPropertyToString(property);
                    var stringMethodExpression = GetStringMethodExpression(operand, value.ToString(), propertyAsString);

                    if (operand.StartsWith("_n"))
                    {
                        return Expression.Not(stringMethodExpression);
                    }

                    return stringMethodExpression;
                }
            case "_in" or "_nin":
                {

                    if (!(value is Array valueArray))
                    {
                        throw new IncorrectTypeForOperandException(operand, "an array");
                    }

                    var convertedValue = new object[valueArray.Length];
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        convertedValue[i] = ConvertValue(property.Type, valueArray.GetValue(i));
                    }

                    var typedArray = Array.CreateInstance(property.Type, convertedValue.Length);
                    Array.Copy(convertedValue, typedArray, convertedValue.Length);

                    var containsMethod = typeof(Enumerable).GetMethods().Where(x => x.Name == "Contains")
                        .Single(x => x.GetParameters().Length == 2).MakeGenericMethod(property.Type);

                    if (containsMethod is null)
                    {
                        throw new GettingMethodByReflectionException("Contains", nameof(Enumerable));
                    }

                    var arrayContainsExpression = Expression.Call(containsMethod, Expression.Constant(typedArray), property);

                    if (operand.StartsWith("_n"))
                    {
                        return Expression.Not(arrayContainsExpression);
                    }

                    return arrayContainsExpression;
                }

            case "_between" or "_nbetween":
                {
                    value = ConvertValue(property.Type, value);

                    return value switch
                    {
                        ValueTuple<double, double> tuple => GetBetweenExpression(property, operand, tuple.Item1,
                            tuple.Item2),
                        ValueTuple<int, int> tuple => GetBetweenExpression(property, operand, tuple.Item1,
                            tuple.Item2),
                        ValueTuple<DateTime, DateTime> tuple => GetBetweenExpression(property, operand, tuple.Item1,
                            tuple.Item2),
                        _ => throw new IncorrectTypeForOperandException(operand, "a tuple of numbers or dates")
                    };

                }
            case "_gt" or "_gte" or "_lt" or "_lte":
                {
                    if (property.Type == typeof(string) || value is string)
                    {
                        throw new IncorrectTypeForOperandException(operand, "number or date");
                    }

                    return operand switch
                    {
                        "_gt" => Expression.GreaterThan(property, Expression.Constant(ConvertValue(property.Type, value))),
                        "_gte" => Expression.GreaterThanOrEqual(property, Expression.Constant(ConvertValue(property.Type, value))),
                        "_lt" => Expression.LessThan(property, Expression.Constant(ConvertValue(property.Type, value))),
                        "_lte" => Expression.LessThanOrEqual(property, Expression.Constant(ConvertValue(property.Type, value))),
                        _ => throw new ArgumentOutOfRangeException(nameof(operand), operand, null)
                    };
                }
            case "_null":
                return Expression.Equal(property, Expression.Constant(null));
            case "_nnull":
                return Expression.NotEqual(property, Expression.Constant(null));
            case "_empty" or "_nempty":
                var arrayLengthExpression = Expression.ArrayLength(property);
                return operand == "_empty"
                    ? Expression.Equal(arrayLengthExpression, Expression.Constant(0))
                    : Expression.GreaterThan(arrayLengthExpression, Expression.Constant(0));
            case "_submitted":
                return Expression.NotEqual(property, Expression.Constant(null));
            case "_regex":
                var rPropertyAsString = ConvertPropertyToString(property);
                var rrValue = value.ToString();
                var regex = new Regex(rrValue ?? string.Empty, RegexOptions.Compiled);
                var isMatchMethod =
                    typeof(Regex).GetMethod("IsMatch", new[] { typeof(string) });
                if (isMatchMethod == null)
                {
                    throw new GettingMethodByReflectionException("IsMatch", nameof(Regex));
                }

                return Expression.Call(Expression.Constant(regex), isMatchMethod, rPropertyAsString);
            default:
                throw new ArgumentOutOfRangeException($"There is no resolver for {operand}");
        }
    }

    private static Expression GetBetweenExpression(Expression property, string operand, object left, object right)
    {
        if (operand == "_nbetween")
        {
            return Expression.AndAlso(
                Expression.LessThan(property, Expression.Constant(left)),
                Expression.GreaterThan(property, Expression.Constant(right)));
        }

        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(property, Expression.Constant(left)),
            Expression.LessThanOrEqual(property, Expression.Constant(right)));
    }

    private static MethodCallExpression GetStringMethodExpression(
        string operand, string? value, Expression propertyAsString)
    {
        var method = operand switch
        {
            "_contains" or "_ncontains" => typeof(string).GetMethod("Contains", new[] { typeof(string) }),
            "_starts_with" or "_nstarts_with" => typeof(string).GetMethod("StartsWith",
                new[] { typeof(string) }),
            "_ends_with" or "_nends_with" => typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
            _ => throw new ArgumentOutOfRangeException(nameof(operand), operand, null)
        };
        if (method is null)
        {
            throw new GettingMethodByReflectionException("string");
        }

        var expression = Expression.Call(propertyAsString, method, Expression.Constant(value));
        return expression;
    }

    private static MethodCallExpression ConvertPropertyToDouble(Expression prop)
        => UseConvertMethodForProp("ToDouble", prop);


    private static MethodCallExpression ConvertPropertyToString(Expression prop)
        => UseConvertMethodForProp("ToString", prop);

    private static MethodCallExpression UseConvertMethodForProp(string methodName, Expression prop)
    {
        var changeTypeMethod = typeof(Convert).GetMethod(methodName, new[] { prop.Type });
        if (changeTypeMethod is null)
        {
            throw new GettingMethodByReflectionException(methodName, nameof(Convert));
        }

        var callExpressionReturningObject = Expression.Call(changeTypeMethod, prop);
        return callExpressionReturningObject;
    }
}