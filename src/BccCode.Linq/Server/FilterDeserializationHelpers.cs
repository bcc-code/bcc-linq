using Newtonsoft.Json;

namespace BccCode.Linq.Server;

public static class FilterDeserializationHelpers
{
    public static Dictionary<string, object> DeserializeJsonRule(string json)
    {
        try
        {
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (result is null)
            {
                throw new ArgumentException("JSON filter rule is invalid.");
            }

            return result;
        }
        catch (Exception)
        {
            throw new ArgumentException("JSON filter rule is invalid.");
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
                    var serializedResultValue = JsonConvert.SerializeObject(result[keyValuePair.Key]);
                    var serializedCurrentDictionaryValue = JsonConvert.SerializeObject(keyValuePair.Value);

                    var deserializedResultValue =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedResultValue);
                    var deserializeCurrentDictionaryValue =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedCurrentDictionaryValue);

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
}
