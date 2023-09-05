using System.Collections;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace BccCode.ApiClient.Immutable;

[DataContract(Name = "Metadata")]
public class ImmutableMetadata : IMetadata
{
    private readonly ImmutableDictionary<string, object> _dict;

    public ImmutableMetadata(IReadOnlyDictionary<string, object> metadata)
    {
        _dict = metadata.ToImmutableDictionary();
    }

    public ImmutableMetadata(IMetadata metadata)
    {
        _dict = metadata.ToImmutableDictionary();
    }

    public int Limit
    {
        get
        {
            if (!_dict.TryGetValue("limit", out var limit))
                return default;

            // We support only type `int` here, but JSON serializer writes it as long.
            // Therefore we need this cast logic here. 
            if (limit is long limitLong)
                return (int)limitLong;

            return ((int?)limit) ?? default;
        }
    }

    public int Skipped
    {
        get
        {
            if (!_dict.TryGetValue("skipped", out var skipped))
                return default;

            // We support only type `int` here, but JSON serializer writes it as long.
            // Therefore we need this cast logic here.
            if (skipped is long skippedLong)
                return (int)skippedLong;

            return ((int?)skipped) ?? default;
        }
    }

    [Obsolete($"Please use property {nameof(FilterCount)} instead.")]
    public long Total
    {
        get
        {
            if (_dict.TryGetValue("filter_count", out object filterCount))
            {
                if (filterCount is int filterCountInt)
                    return filterCountInt;

                return (long)filterCount;
            }

            if (_dict.TryGetValue("total", out object total))
            {
                if (total is int totalInt)
                    return totalInt;

                return (long)total;
            }

            return default;
        }
    }

    public long? FilterCount => _dict.TryGetValue("filter_count", out var filterCount) ? (long?)filterCount : Total;
    public long? TotalCount => _dict.TryGetValue("total_count", out var totalCount) ? (long?)totalCount : null;

    #region IEnumerable<KeyValuePair<string, object>>
    
    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
    {
        return _dict.GetEnumerator();
    }
    
    int IReadOnlyCollection<KeyValuePair<string, object>>.Count => _dict.Count;

    bool IReadOnlyDictionary<string, object>.ContainsKey(string key)
    {
        return _dict.ContainsKey(key);
    }

    bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value)
    {
        return _dict.TryGetValue(key, out value);
    }

    object IReadOnlyDictionary<string, object>.this[string key] => _dict[key];
    
    IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => _dict.Keys;

    IEnumerable<object> IReadOnlyDictionary<string, object>.Values => _dict.Values;

    #endregion

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_dict).GetEnumerator();
    }

    #endregion
}
