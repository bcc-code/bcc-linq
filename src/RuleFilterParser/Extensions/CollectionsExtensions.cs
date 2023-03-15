namespace RuleFilterParser.Extensions;

public static class CollectionsExtensions
{
    public static IEnumerable<T> ApplyRuleFilter<T>(this IEnumerable<T> source, Filter filter)
    {
        var exp = FilterToLambdaParser.Parse<T>(filter);
        return source.Where(exp.Compile());
    }
}