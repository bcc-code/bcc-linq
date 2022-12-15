using System.Linq.Expressions;

namespace RuleFilterParser;

public static class FilterToLambdaParser
{
    public static Expression<Func<T, bool>> Parse<T>(Filter filter)
    {
        try
        {
            var parameter = Expression.Parameter(typeof(T), typeof(T).FullName?.ToLower());
            var filterExpressionsList = GetExpressionsForFilter(filter, parameter);
            
            var expressionBody = MergeExpressions(filterExpressionsList, LogicalFilter.And);

            var lambda = Expression.Lambda<Func<T, bool>>(expressionBody, parameter);
            return lambda;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static List<Expression> GetExpressionsForFilter(
        Filter filter,
        ParameterExpression parameter,
        string? parentKey = null)
    {
        var allExpressions = new List<Expression>();

        foreach (var prop in filter.Properties)
        {
            if (prop.Value is Filter propertyFilter)
            {
                switch (prop.Key)
                {
                    case "_and":
                    {
                        var andExpressions = new List<Expression>();

                        var nextLevelExpressions = GetExpressionsForFilter(propertyFilter, parameter);
                        if (nextLevelExpressions.Count > 0)
                        {
                            andExpressions.AddRange(nextLevelExpressions);
                        }

                        var finalAndExpression = MergeExpressions(andExpressions, LogicalFilter.And);
                        allExpressions.Add(finalAndExpression);
                        break;
                    }
                    case "_or":
                    {
                        var orExpressions = new List<Expression>();

                        var nextLevelExpressions = GetExpressionsForFilter(propertyFilter, parameter);
                        if (nextLevelExpressions.Count > 0)
                        {
                            orExpressions.AddRange(nextLevelExpressions);
                        }

                        var finalOrExpression = MergeExpressions(orExpressions, LogicalFilter.Or);
                        allExpressions.Add(finalOrExpression);
                        break;
                    }
                    default:
                        allExpressions.AddRange(GetExpressionsForFilter(propertyFilter, parameter, prop.Key));
                        break;
                }
            }
            else if (prop.Key.StartsWith("_"))
            {
                var expressionForProperty = OperandToExpressionResolver.GetExpressionForRule(
                    Expression.Property(parameter,
                        parentKey ?? throw new ArgumentNullException(nameof(parentKey))),
                    prop.Key,
                    prop.Value);
                allExpressions.Add(expressionForProperty);
            }
        }

        return allExpressions;
    }

    private static Expression MergeExpressions(IReadOnlyList<Expression> main, LogicalFilter @operator)
    {
        if (main.Count == 0)
        {
            throw new ArgumentException("Could not merge expressions. It looks like Filter is incorrect.");
        }

        var final = main[0];

        switch (main.Count)
        {
            case 2:
                final = @operator switch
                {
                    LogicalFilter.And => Expression.AndAlso(main[0], main[1]),
                    LogicalFilter.Or => Expression.Or(main[0], main[1]),
                    _ => final
                };

                break;
            case > 2:
            {
                for (var i = 1; i < main.Count; i++)
                {
                    final = @operator switch
                    {
                        LogicalFilter.And => Expression.AndAlso(final, main[i]),
                        LogicalFilter.Or => Expression.Or(final, main[i]),
                        _ => final
                    };
                }

                break;
            }
        }

        return final;
    }
}