using System.Linq.Expressions;
using System.Text.RegularExpressions;
using RuleFilterParser.Exceptions;

namespace RuleFilterParser;

public static class OperandToExpressionResolver
{
    public static Expression GetExpressionForRule(Expression property, string operand, object value)
    {
        if (double.TryParse(value.ToString(), out var doubleValue))
        {
            property = ConvertPropertyToDouble(property);
            value = doubleValue;
        }
        else if (bool.TryParse(value.ToString(), out var boolValue))
        {
            property = ConvertPropertyToBoolean(property);
            value = boolValue;
        }
        else if (value is int[] arr)
        {
            property = ConvertPropertyToDouble(property);
            value = Array.ConvertAll<int, double>(arr, x => x);
        }
        else if (DateTime.TryParse(value.ToString(), out var dateTime) && property.Type == typeof(DateTime))
        {
            // property = ConvertPropertyToDateTime(property);
            value = dateTime;
        }

        if (property.Type == typeof(int) || property.Type == typeof(long))
        {
            property = ConvertPropertyToDouble(property);
        }

        switch (operand)
        {
            case "_eq":
                return Expression.Equal(property, Expression.Constant(value));
            case "_neq":
                return Expression.NotEqual(property, Expression.Constant(value));
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
                if (value is not IEnumerable<object>)
                {
                    throw new IncorrectTypeForOperandException(operand, "an array");
                }

                var containsMethod = typeof(Enumerable).GetMethods().Where(x => x.Name == "Contains")
                    .Single(x => x.GetParameters().Length == 2).MakeGenericMethod(typeof(object));

                if (containsMethod is null)
                {
                    throw new GettingMethodByReflectionException("Contains", nameof(Enumerable));
                }

                var arrayContainsExpression =
                    Expression.Call(containsMethod, Expression.Constant(value), property);

                if (operand.StartsWith("_n"))
                {
                    return Expression.Not(arrayContainsExpression);
                }

                return arrayContainsExpression;
            }

            case "_between" or "_nbetween":
            {
                if (value is ValueTuple<int, int> intTuple)
                {
                    property = ConvertPropertyToDouble(property);
                    value = new ValueTuple<double, double>(
                        Convert.ToDouble(intTuple.Item1),
                        Convert.ToDouble(intTuple.Item2));
                }

                if (value is ValueTuple<string, string> stringTuple)
                {
                    property = ConvertPropertyToDouble(property);
                    value = new ValueTuple<double, double>(
                        Convert.ToDouble(stringTuple.Item1),
                        Convert.ToDouble(stringTuple.Item2));
                }

                if (value is ValueTuple<DateTime, DateTime> dtTuple && property.Type == typeof(DateTime))
                {
                    value = new ValueTuple<DateTime, DateTime>(
                        Convert.ToDateTime(dtTuple.Item1),
                        Convert.ToDateTime(dtTuple.Item2));
                }

                switch (value)
                {
                    case ValueTuple<double, double> doubleTuple:
                    {
                        var (left, right) = doubleTuple;
                        return GetBetweenExpression(property, operand, left, right);
                    }
                    case ValueTuple<DateTime, DateTime> dateTimeTuple:
                    {
                        var (left, right) = dateTimeTuple;
                        return GetBetweenExpression(property, operand, left, right);
                    }
                    default:
                    {
                        throw new IncorrectTypeForOperandException(operand, "a tuple of numbers or dates");
                    }
                }
            }
            case "_gt" or "_gte" or "_lt" or "_lte":
            {
                if (property.Type == typeof(string) || value is string)
                {
                    throw new IncorrectTypeForOperandException(operand, "number or date");
                }

                return operand switch
                {
                    "_gt" => Expression.GreaterThan(property, Expression.Constant(value)),
                    "_gte" => Expression.GreaterThanOrEqual(property, Expression.Constant(value)),
                    "_lt" => Expression.LessThan(property, Expression.Constant(value)),
                    "_lte" => Expression.LessThanOrEqual(property, Expression.Constant(value)),
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


    private static MethodCallExpression ConvertPropertyToBoolean(Expression prop)
        => UseConvertMethodForProp("ToBoolean", prop);


    private static MethodCallExpression ConvertPropertyToString(Expression prop)
        => UseConvertMethodForProp("ToString", prop);

    private static MethodCallExpression ConvertPropertyToDateTime(Expression prop)
        => UseConvertMethodForProp("ToDateTime", prop);

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