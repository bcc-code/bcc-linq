using System.Collections;
using System.Runtime.Serialization;

namespace BccCode.ApiClient;

[DataContract(Name = "Metadata")]
public class Metadata : IMetadata, IDictionary, IDictionary<string, object>
{
    private readonly IDictionary<string, object> _dict;

    public Metadata()
    {
        _dict = new Dictionary<string, object>();
    }

    public Metadata(IReadOnlyDictionary<string, object>? dict)
    {
        if (dict is Dictionary<string, object> dictTypeRight)
            _dict = dictTypeRight;

        _dict = new Dictionary<string, object>(dict);
    }
    
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _dict.GetEnumerator();
    }

    /// <summary>
    /// Returns the page limit of the request.
    /// </summary>
    public int Limit
    {
        get
        {
            if (!this.TryGetValue("limit", out var limit))
                return default;

            // We support only type `int` here, but JSON serializer writes it as long.
            // Therefore we need this cast logic here. 
            if (limit is long limitLong)
                return (int)limitLong;

            return ((int?)limit) ?? default;
        }
    }

    /// <summary>
    /// Returns how many rows have been skipped from the start.
    /// </summary>
    public int Skipped
    {
        get
        {
            if (!this.TryGetValue("skipped", out var skipped))
                return default;

            // We support only type `int` here, but JSON serializer writes it as long.
            // Therefore we need this cast logic here.
            if (skipped is long skippedLong)
                return (int)skippedLong;

            return ((int?)skipped) ?? default;
        }
    }

    /// <summary>
    /// Returns the number of rows, passed by the metadata property <c>"total"</c>.
    /// </summary>
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

    /// <summary>
    /// Returns the item count of the collection you're querying, taking the current filter/search parameters into account
    /// if given, otherwise <c>null</c>.
    /// </summary>
    public long? FilterCount =>
#pragma warning disable CS0618
        this.TryGetValue("filter_count", out var filterCount) ? (long?)filterCount : Total;
#pragma warning restore CS0618

    /// <summary>
    /// Returns the total item count of the collection you're querying if given, otherwise <c>null</c>.
    /// </summary>
    public long? TotalCount =>
        this.TryGetValue("total_count", out var totalCount) ? (long?)totalCount : null;

    public void Add(string key, object value)
    {
        _dict.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return _dict.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return _dict.Remove(key);
    }

    public bool TryGetValue(string key, out object value) => _dict.TryGetValue(key, out value);

    public object this[string key] => _dict[key] ?? throw new KeyNotFoundException();


    #region IDictionary<string, object>

    ICollection<object> IDictionary<string, object>.Values => _dict.Values;

    ICollection<string> IDictionary<string, object>.Keys => _dict.Keys;

    object IDictionary<string, object>.this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }

    #endregion
    
    #region ICollection<KeyValuePair<string, object>>

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
        _dict.Add(item);
    }

    void ICollection<KeyValuePair<string, object>>.Clear()
    {
        _dict.Clear();
    }

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
    {
        return _dict.Contains(item);
    }

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        _dict.CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
        return _dict.Remove(item);
    }

    int ICollection<KeyValuePair<string, object>>.Count => _dict.Count;

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly => _dict.IsReadOnly;

    #endregion
    
    #region IReadOnlyCollection<KeyValuePair<string, object>>

    int IReadOnlyCollection<KeyValuePair<string, object>>.Count => ((IReadOnlyCollection<KeyValuePair<string, object>>)_dict).Count;

    IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => _dict.Keys;

    IEnumerable<object> IReadOnlyDictionary<string, object>.Values => _dict.Values;

    #endregion
    
    #region IDictionary

    void IDictionary.Add(object key, object value)
    {
        ((IDictionary)_dict).Add(key, value);
    }

    void IDictionary.Clear()
    {
        _dict.Clear();
    }

    bool IDictionary.Contains(object key)
    {
        return ((IDictionary)_dict).Contains(key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return ((IDictionary)_dict).GetEnumerator();
    }

    void IDictionary.Remove(object key)
    {
        ((IDictionary)_dict).Remove(key);
    }
    bool IDictionary.IsFixedSize => ((IDictionary)_dict).IsFixedSize;

    bool IDictionary.IsReadOnly => ((IDictionary)_dict).IsReadOnly;

    object IDictionary.this[object key]
    {
        get => ((IDictionary)_dict)[key];
        set => ((IDictionary)_dict)[key] = value;
    }

    #endregion

    #region ICollection
    
    int ICollection.Count => ((ICollection)_dict).Count;

    void ICollection.CopyTo(Array array, int index)
    {
        ((ICollection)_dict).CopyTo(array, index);
    }

    bool ICollection.IsSynchronized => ((ICollection)_dict).IsSynchronized;

    object ICollection.SyncRoot => ((ICollection)_dict).SyncRoot;

    ICollection IDictionary.Values => ((IDictionary)_dict).Values;

    ICollection IDictionary.Keys => ((IDictionary)_dict).Keys;

    #endregion

    #region IEnumerator
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _dict.GetEnumerator();
    }

    #endregion
}
