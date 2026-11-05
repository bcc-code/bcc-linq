using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using BccCode.Linq.Server.Exceptions;

namespace BccCode.Linq.Server;

public static class OperandToExpressionResolver
{

    public static object? ConvertValue(Type type, object? value)
    {
        if (value == null)
            return null;

        var valueType = value.GetType();
        if (valueType == type)
            return value;

        Type? nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType != null)
        {
            if (value is string strValue && strValue == "null")
                return null;

            var newValue = ConvertValue(nullableType, value);

            return nullableType switch
            {
                Type when nullableType == typeof(bool) => (bool?)newValue,
                Type when nullableType == typeof(sbyte) => (sbyte?)newValue,
                Type when nullableType == typeof(byte) => (byte?)newValue,
                Type when nullableType == typeof(char) => (char?)newValue,
                Type when nullableType == typeof(short) => (short?)newValue,
                Type when nullableType == typeof(ushort) => (ushort?)newValue,
                Type when nullableType == typeof(int) => (int?)newValue,
                Type when nullableType == typeof(uint) => (uint?)newValue,
                Type when nullableType == typeof(long) => (long?)newValue,
                Type when nullableType == typeof(ulong) => (ulong?)newValue,
                Type when nullableType == typeof(nint) => (nint?)newValue,
                Type when nullableType == typeof(nuint) => (nuint?)newValue,
                Type when nullableType == typeof(float) => (float?)newValue,
                Type when nullableType == typeof(double) => (double?)newValue,
                Type when nullableType == typeof(decimal) => (decimal?)newValue,
                Type when nullableType == typeof(Guid) => (Guid?)newValue,
                Type when nullableType == typeof(DateTime) => (DateTime?)newValue,
                Type when nullableType == typeof(TimeSpan) => (TimeSpan?)newValue,
#if NET6_0_OR_GREATER
                Type when nullableType == typeof(DateOnly) => (DateOnly?)newValue,
                Type when nullableType == typeof(TimeOnly) => (TimeOnly?)newValue,
#endif
                _ => newValue,
            };
        }

        // converts value to an array
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
        {
            // run for an array
            if (value is not IEnumerable valueEnumerable)
            {
                throw new InvalidCastException(
                    $"Value {value} of type {valueType.FullName} does not implement {typeof(IEnumerable)}. It cannot be casted to {type.FullName}");
            }

            var listBaseType = type.GenericTypeArguments[0];
            var listType = typeof(List<>).MakeGenericType(listBaseType);
            var list = Activator.CreateInstance(listType);

            var listAddMethodInfo = listType.GetMethod(nameof(List<object>.Add), BindingFlags.Public | BindingFlags.Instance);
            Debug.Assert(listAddMethodInfo != null);

            foreach (var itemValue in valueEnumerable)
            {
                listAddMethodInfo.Invoke(list,
                    new[] { ConvertValue(listBaseType, itemValue) }
                );
            }

            var toArrayMethodInfo = listType.GetMethod(nameof(List<object>.ToArray), BindingFlags.Public | BindingFlags.Instance);
            Debug.Assert(toArrayMethodInfo != null);

            return toArrayMethodInfo.Invoke(list, Array.Empty<object>());
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
        {
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                // ReSharper disable once PossibleNullReferenceException
                return type.GetConstructor(new[] { type.GenericTypeArguments[0], type.GenericTypeArguments[1] })
                    .Invoke(new[]
                    {
                        ConvertValue(
                            type.GenericTypeArguments[0],
                            valueType.GetField(nameof(ValueTuple<int, int>.Item1)).GetValue(value)
                        ),
                        ConvertValue(
                            type.GenericTypeArguments[1],
                            valueType.GetField(nameof(ValueTuple<int, int>.Item1)).GetValue(value)
                        )
                    });
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Tuple<,>))
            {
                return type.GetConstructor(new[] { type.GenericTypeArguments[0], type.GenericTypeArguments[1] })
                    .Invoke(new[]
                    {
                        ConvertValue(
                            type.GenericTypeArguments[0],
                            valueType.GetProperty(nameof(Tuple<object, object>.Item1)).GetValue(value)
                        ),
                        ConvertValue(
                            type.GenericTypeArguments[1],
                            valueType.GetProperty(nameof(Tuple<object, object>.Item1)).GetValue(value)
                        )
                    });
            }
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,>))
        {
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                return Tuple.Create(
                    ConvertValue(
                        type.GenericTypeArguments[0],
                        valueType.GetField(nameof(ValueTuple<int, int>.Item1)).GetValue(value)
                    ),
                    ConvertValue(
                        type.GenericTypeArguments[1],
                        valueType.GetField(nameof(ValueTuple<int, int>.Item1)).GetValue(value)
                    )
                );
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Tuple<,>))
            {
                return Tuple.Create(
                    ConvertValue(
                        type.GenericTypeArguments[0],
                        // ReSharper disable once PossibleNullReferenceException
                        valueType.GetProperty(nameof(Tuple<object, object>.Item1)).GetValue(value)
                    ),
                    ConvertValue(
                        type.GenericTypeArguments[1],
                        // ReSharper disable once PossibleNullReferenceException
                        valueType.GetProperty(nameof(Tuple<object, object>.Item1)).GetValue(value)
                    )
                );
            }
        }

        try
        {
            value = type switch
            {
                Type when type == typeof(bool) && value is string strVal => strVal == "1",
                Type when type == typeof(int) && value is string strVal => (int)double.Parse(strVal, CultureInfo.InvariantCulture),
                Type when type == typeof(float) && value is string strVal => float.Parse(strVal, CultureInfo.InvariantCulture),
                Type when type == typeof(double) && value is string strVal => double.Parse(strVal, CultureInfo.InvariantCulture),
                Type when type == typeof(decimal) && value is string strVal => decimal.Parse(strVal, CultureInfo.InvariantCulture),
                Type when type == typeof(Guid) && value is string strVal && Guid.TryParse(strVal, out var uuid) => uuid,
                Type when type == typeof(DateTime) && value is string strVal && DateTime.TryParse(strVal, out var dateTime) => dateTime,
                Type when type == typeof(TimeSpan) && value is string strVal && TimeSpan.TryParse(strVal, out var dateTime) => dateTime,
#if NET6_0_OR_GREATER
                Type when type == typeof(DateOnly) && value is string strVal && DateOnly.TryParse(strVal, out var dateTime) => dateTime,
                Type when type == typeof(TimeOnly) && value is string strVal && TimeOnly.TryParse(strVal, out var dateTime) => dateTime,
#endif
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
            throw new InvalidCastException(
                $"Could not cast value {value} (of type {valueType.FullName}) to {type.FullName}");
        }

        return value;
    }

    public static Expression GetExpressionForRule(Expression property, string operand, object value)
    {

        var propertyType = property.Type;
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
                    for (var i = 0; i < valueArray.Length; i++)
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
                    value = ConvertValue(typeof(ValueTuple<,>).MakeGenericType(property.Type, property.Type), value);

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
