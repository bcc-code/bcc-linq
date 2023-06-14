using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BccCode.Linq.Extensions;

namespace BccCode.Linq;

public abstract class Filter
{
    protected Dictionary<string, object> _properties = new();
    public Dictionary<string, object> Properties => _properties;
    public Filter GetInvertedFilter() => FilterInverter.Invert(this);
    public void RemoveFieldFromFilter(string field, string path = "", string history = "")
    {
        if (_properties.Count == 0)
        {
            return;
        }

        foreach (var key in _properties.Keys.ToList())
        {
            if (key == field && (path == "" || history.EndsWith(path)))
            {
                _properties.Remove(field);
                return;
            }

            if (_properties[key] is not Filter filter)
            {
                continue;
            }

            history = $"{history}.{key}";
            filter.RemoveFieldFromFilter(field, path, history);
        }
    }

    public void RenameFieldInFilter(string oldField, string newField, string path = "", string history = "")
    {
        if (_properties.Count == 0)
        {
            return;
        }

        foreach (var key in _properties.Keys.ToList())
        {
            if (key == oldField && (path == "" || history.EndsWith(path)))
            {
                _properties.RenameKey(oldField, newField);
                return;
            }

            if (_properties[key] is not Filter filter)
            {
                continue;
            }

            history = $"{history}.{key}";
            filter.RenameFieldInFilter(oldField, newField, path, history);
        }
    }
}
public class Filter<T> : Filter
{

    public Filter() : this("{}")
    {
    }

    public Filter(string json) => _parse(json);

    private Filter(object obj) : this(JsonConvert.SerializeObject(obj))
    {
    }

    public Type GetFilterType() => typeof(T);

    private void _parse(string json)
    {
        var deserializedJson = FilterDeserializationHelpers.DeserializeJsonRule(json);

        var propertyMetadata = typeof(T).GetProperties();

        foreach (var (key, value) in deserializedJson)
        {
            var propertyInfo = propertyMetadata.FirstOrDefault(x => x.Name.ToLower() == key.ToLower());

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
                deserializedJson[key] = new Filter<T>(result);
            }
            else if (new[] { "_in", "_nin" }.Contains(key))
            {
                var array = JsonConvert.DeserializeObject<T[]>(value.ToString() ?? string.Empty);
                if (array is null || array.Length == 0)
                {
                    throw new ArgumentException(
                        $"JSON filter rule is invalid. Array under {key} is null or empty.");
                }

                deserializedJson[key] = array;

            }
            else if (new[] { "_between", "_nbetween" }.Contains(key))
            {
                var array = JsonConvert.DeserializeObject<object[]>(value.ToString() ?? string.Empty);
                if (array is null || array.Length != 2)
                {
                    throw new ArgumentException(
                        $"JSON filter rule is invalid. Value under {key} is not pair.");
                }
                deserializedJson[key] = OperandToExpressionResolver.ConvertValue(propertyInfo?.PropertyType ?? GetFilterType(), (array[0].ToString(), array[1].ToString()));

            }
            else if (key.StartsWith("_"))
            {
                deserializedJson[key] = OperandToExpressionResolver.ConvertValue(propertyInfo?.PropertyType ?? GetFilterType(), value);
            }
            else
            {
                if (propertyInfo != null)
                {
                    Type filterType = typeof(Filter<>).MakeGenericType(propertyInfo.PropertyType);
                    try
                    {
                        deserializedJson[key] = Activator.CreateInstance(filterType, value.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw e.InnerException;
                    }

                }
            }
        }

        _properties = deserializedJson;
    }



}
