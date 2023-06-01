namespace RuleFilterParser.Extensions;

public static class DictionaryExtensions
{
    public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
        TKey fromKey, TKey toKey)
    {
        var value = dic[fromKey];
        dic.Remove(fromKey);
        dic[toKey] = value;
    }
}
