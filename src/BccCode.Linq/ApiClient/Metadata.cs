using System.Collections;
using BccCode.Linq.ApiClient;

namespace BccCode.Linq.Tests;

public class Metadata : IMetadata
{
    private readonly IReadOnlyDictionary<string, object>? _dict;

    public Metadata()
    {
        _dict = new Dictionary<string, object>();
    }

    public Metadata(IReadOnlyDictionary<string, object>? dict)
    {
        _dict = dict;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _dict?.GetEnumerator() ?? Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _dict?.GetEnumerator() ?? Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();
    }

    public int Count => _dict?.Count ?? 0;
    public bool ContainsKey(string key)
    {
        return _dict?.ContainsKey(key) ?? false;
    }

    public bool TryGetValue(string key, out object? value)
    {
        if (_dict == null)
        {
            value = default;
            return false;
        }
        
        return _dict.TryGetValue(key, out value);
    }

    public object this[string key] => _dict?[key] ?? throw new KeyNotFoundException();

    public IEnumerable<string> Keys => _dict?.Keys ?? Enumerable.Empty<string>();
    public IEnumerable<object> Values => _dict?.Values ?? Enumerable.Empty<object>();
}
