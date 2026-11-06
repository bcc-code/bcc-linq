using System.Text.Json;
using System.Text.Json.Serialization;

namespace BccCode.Linq.Server;

public static class FilterDeserializationHelpers
{
    /// <summary>
    /// Options shared by all JSON (de)serialization of filters. They keep the leniency the filter
    /// parsing had before: case-insensitive names, numbers given as strings, enums by name or number.
    /// </summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter(), new LenientStringConverter() },
    };

    /// <summary>
    /// Reads numbers and booleans into string values, as Newtonsoft.Json did, so that for example
    /// <c>{"name": {"_in": [1, 2]}}</c> works on a string property.
    /// </summary>
    private sealed class LenientStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => System.Text.Encoding.UTF8.GetString(reader.ValueSpan),
                JsonTokenType.True => "True",
                JsonTokenType.False => "False",
                JsonTokenType.Null => null,
                _ => throw new JsonException($"Cannot convert {reader.TokenType} to a string."),
            };

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value);
    }

    /// <summary>
    /// Deserializes a filter rule. The values of the returned dictionary are <see cref="JsonElement"/>s.
    /// </summary>
    public static Dictionary<string, object> DeserializeJsonRule(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions);

            if (result is null)
            {
                throw new ArgumentException("JSON filter rule is invalid.");
            }

            return result;
        }
        catch (Exception e)
        {
            throw new ArgumentException("JSON filter rule is invalid.", e);
        }
    }

    public static Dictionary<string, object> MergeDictionaries(IEnumerable<Dictionary<string, object>> dictionaries)
    {
        var result = new Dictionary<string, object>();

        foreach (var dictionary in dictionaries)
        {
            foreach (var keyValuePair in dictionary)
            {
                if (result.ContainsKey(keyValuePair.Key) == false)
                {
                    result.Add(keyValuePair.Key, keyValuePair.Value);
                }
                else
                {
                    var serializedResultValue = JsonSerializer.Serialize(result[keyValuePair.Key], JsonOptions);
                    var serializedCurrentDictionaryValue = JsonSerializer.Serialize(keyValuePair.Value, JsonOptions);

                    var deserializedResultValue =
                        JsonSerializer.Deserialize<Dictionary<string, object>>(serializedResultValue, JsonOptions);
                    var deserializeCurrentDictionaryValue =
                        JsonSerializer.Deserialize<Dictionary<string, object>>(serializedCurrentDictionaryValue, JsonOptions);

                    if (deserializedResultValue is null || deserializeCurrentDictionaryValue is null)
                    {
                        throw new JsonException(
                            "Could not merge dictionaries. There is error during deserializing one of them.");
                    }

                    result[keyValuePair.Key] = MergeDictionaries(
                        new[] { deserializedResultValue, deserializeCurrentDictionaryValue });
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a scalar <see cref="JsonElement"/> into the .NET value the filter conversion works on:
    /// string, long or double (decimal when <paramref name="targetType"/> is decimal), bool or null.
    /// ISO 8601 strings are returned as <see cref="DateTime"/> when the target is a DateTime.
    /// Arrays and objects are returned unchanged.
    /// </summary>
    internal static object? UnwrapJsonElement(JsonElement element, Type targetType)
    {
        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;

        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                if (type == typeof(DateTime) && element.TryGetDateTime(out var dateTime))
                {
                    return dateTime;
                }

                return element.GetString();
            case JsonValueKind.Number:
                if (type == typeof(decimal) && element.TryGetDecimal(out var decimalValue))
                {
                    return decimalValue;
                }

                return element.TryGetInt64(out var longValue) ? longValue : (object)element.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                return element;
        }
    }

    /// <summary>
    /// Value of one side of a between operand as it is handed to the value conversion: the text of the
    /// element, or a <see cref="DateTime"/> for ISO 8601 strings when the target is a DateTime.
    /// </summary>
    internal static object? ToBetweenOperand(Type targetType, object? value)
    {
        if (value is not JsonElement element)
        {
            return value?.ToString();
        }

        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (type == typeof(DateTime) && element.ValueKind == JsonValueKind.String &&
            element.TryGetDateTime(out var dateTime))
        {
            return dateTime;
        }

        return element.ToString();
    }
}
