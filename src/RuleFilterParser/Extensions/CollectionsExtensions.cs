namespace RuleFilterParser.Extensions;

public static class CollectionsExtensions
{
    public static IEnumerable<T> ApplyRuleFilter<T>(this IEnumerable<T> source, Filter<T> filter) where T : class
    {
        var exp = FilterToLambdaParser.Parse(filter);
        return source.Where(exp.Compile());
    }

    public static IQueryable<T> ApplyRuleFilter<T>(this IQueryable<T> source, Filter<T> filter) where T : class
    {
        var exp = FilterToLambdaParser.Parse(filter);
        return source.Where(exp);
    }
}