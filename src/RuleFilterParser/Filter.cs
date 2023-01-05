using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuleFilterParser.Exceptions;

namespace RuleFilterParser;

public class Filter
{
    private Dictionary<string, object> _properties = new();

    public Dictionary<string, object> Properties => _properties;

    public Filter() : this("{}")
    {
    }    
    
    public Filter(string json) => Parse(json);

    private Filter(object obj) : this(JsonConvert.SerializeObject(obj))
    {
    }

    private void Parse(string json)
    {
        var deserializedJson = FilterDeserializationHelpers.DeserializeJsonRule(json);

        foreach (var (key, value) in deserializedJson)
        {
            if (new[] { "_and", "_or" }.Contains(key))
            {
                if (value is not JArray jArray)
                {
                    continue;
                }

                var dictionaries = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jArray.ToString());
                if (dictionaries is null)
                {
                    throw new JsonException("Could not deserialize array of logical filter.");
                }

                var result = FilterDeserializationHelpers.MergeDictionaries(dictionaries);
                deserializedJson[key] = new Filter(result);
            }
            else if (new[] { "_in", "_nin" }.Contains(key))
            {
                var array = JsonConvert.DeserializeObject<object[]>(value.ToString() ?? string.Empty);
                if (array is null || array.Length == 0)
                {
                    throw new ArgumentException(
                        $"JSON filter rule is invalid. Array under {key} is null or empty.");
                }

                deserializedJson[key] = array[0] switch
                {
                    bool => array.Select(x => (bool)x),
                    int or double or long => array.Select(Convert.ToDouble),
                    DateTime => array.Select(x => (DateTime)x),
                    _ => array.Select(x => x.ToString())
                };
            }
            else if (new[] { "_between", "_nbetween" }.Contains(key))
            {
                var array = JsonConvert.DeserializeObject<object[]>(value.ToString() ?? string.Empty);
                if (array is null || array.Length != 2)
                {
                    throw new ArgumentException(
                        $"JSON filter rule is invalid. Value under {key} is not pair.");
                }

                var from = array[0];
                var to = array[1];

                deserializedJson[key] = from switch
                {
                    int or double or long => (Convert.ToDouble(from), Convert.ToDouble(to)),
                    DateTime => ((DateTime)from, (DateTime)to),
                    _ => ((string)from, (string)to)
                };
            }
            else if (key.StartsWith("_"))
            {
                deserializedJson[key] = value switch
                {
                    bool boolValue => boolValue,
                    int or double or long => Convert.ToDouble(value),
                    DateTime => value,
                    _ => (string)value
                };
            }
            else
            {
                deserializedJson[key] = new Filter(value);
            }
        }

        _properties = deserializedJson;
    }

    public Filter GetInvertedFilter() => FilterInverter.Invert(this);

    public void RemoveFieldFromFilter(string field)
    {
        if (_properties.Count == 0)
        {
            return;
        }

        foreach (var key in _properties.Keys.ToList())
        {
            if (key == field)
            {
                _properties.Remove(field);
                return;
            }

            if (_properties[key] is not Filter filter)
            {
                continue;
            }

            filter.RemoveFieldFromFilter(field);
        }
    }

    public void RenameFieldInFilter(string oldField, string newField)
    {
        if (_properties.Count == 0)
        {
            return;
        }

        foreach (var key in _properties.Keys.ToList())
        {
            if (key == oldField)
            {
                _properties.RenameKey(oldField, newField);
                return;
            }

            if (_properties[key] is not Filter filter)
            {
                continue;
            }

            filter.RenameFieldInFilter(oldField, newField);
        }
    }
}