namespace RuleFilterParser;

public static class Extensions
{
    public static IEnumerable<T> ApplyRuleFilter<T>(this IEnumerable<T> source, Filter filter)
    {
        var exp = FilterToLambdaParser.Parse<T>(filter);
        return source.Where(exp.Compile());
    }
    
    public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
        TKey fromKey, TKey toKey)
    {
        TValue value = dic[fromKey];
        dic.Remove(fromKey);
        dic[toKey] = value;
    }
}