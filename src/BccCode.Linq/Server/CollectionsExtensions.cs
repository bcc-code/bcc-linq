using System.Linq.Expressions;
using System.Reflection;
using BccCode.Platform;

namespace BccCode.Linq.Server;

public static class CollectionsExtensions
{
    [Obsolete($"Please use IQueryable<T>.{nameof(ApplyApiRequest)} instead")]
    public static IEnumerable<T> ApplyRuleFilter<T>(this IEnumerable<T> source, Filter<T> filter) where T : class
    {
        var exp = FilterToLambdaParser.Parse(filter);
        return source.Where(exp.Compile());
    }

    [Obsolete($"Please use {nameof(ApplyApiRequest)} instead")]
    public static IQueryable<T> ApplyRuleFilter<T>(this IQueryable<T> source, Filter<T> filter) where T : class
    {
        var exp = FilterToLambdaParser.Parse(filter);
        return source.Where(exp);
    }
    public static IQueryable<T> ApplyApiRequest<T>(this IQueryable<T> source, IQueryableParameters query,
        string? defaultSort = null)
        where T : class
    {
        var type = typeof(T);

        if (query.Filter != null)
        {
            source = source.Where(FilterToLambdaParser.Parse(new Filter<T>(query.Filter)));
        }

        var sort = query.Sort ?? defaultSort;
        if (sort != null)
        {
            var sortByFields = sort.Split(',');

            IOrderedQueryable<T>? orderedSource = null;

            foreach (var sortByField in sortByFields)
            {
                if (sortByField.Contains('.'))
                    throw new NotSupportedException("Sorting on nested properties is not allowed");

                var propertyName = sortByField.Trim();

                if (string.IsNullOrEmpty(propertyName))
                    // sort property not given, wrong call but optimistic handling, ignore it ...
                    continue;

                bool descending;
                if (propertyName[0] == '-')
                {
                    propertyName = propertyName[1..].Trim();

                    if (string.IsNullOrEmpty(propertyName))
                        // sort property not given, wrong call but optimistic handling, ignore it ...
                        continue;

                    // descending sorting
                    descending = true;
                }
                else
                {
                    // ascending sorting
                    descending = false;
                }

                // try find property by camel case
                var propertyInfo = type.GetProperty(nameof(propertyName),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

                if (propertyInfo == null)
                {
                    // transform camel to pascal case
                    if (propertyName.Length == 1)
                    {
                        // ReSharper disable once RedundantAssignment
                        propertyName = propertyName[1].ToString().ToUpper();
                    }
                    else
                    {
                        // ReSharper disable once RedundantAssignment
                        propertyName = propertyName[1].ToString().ToUpper() +
                                       propertyName[2..];
                    }

                    propertyInfo = type.GetProperty(nameof(propertyName),
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                }

                if (propertyInfo == null)
                {
                    throw new InvalidOperationException($"Could not find property for sort field {sortByField}");
                }

                // build key selector
                var param = Expression.Parameter(type);
                var keySelector = Expression.Lambda(
                    Expression.Property(param, propertyInfo),
                    param);

                if (orderedSource == null)
                {
                    var orderByMethodInfo =
                        descending
                            ? System.Linq.Internal.Queryable.OrderByDescendingMethodInfo
                            : System.Linq.Internal.Queryable.OrderByMethodInfo;

                    var method = orderByMethodInfo.MakeGenericMethod(type, propertyInfo.PropertyType);

                    orderedSource = (IOrderedQueryable<T>)source.Provider.CreateQuery(
                        Expression.Call(
                            source.Expression,
                            method,
                            keySelector
                        ));
                }
                else
                {
                    var thenByMethodInfo =
                        descending
                            ? System.Linq.Internal.Queryable.ThenByDescendingMethodInfo
                            : System.Linq.Internal.Queryable.ThenByMethodInfo;

                    var method = thenByMethodInfo.MakeGenericMethod(type, propertyInfo.PropertyType);

                    orderedSource = (IOrderedQueryable<T>)orderedSource.Provider.CreateQuery(
                        Expression.Call(
                            orderedSource.Expression,
                            method,
                            keySelector
                        ));
                }
            }

            if (orderedSource != null)
                source = orderedSource;
        }

        if (query.Offset != null && query.Offset != 0)
        {
            if (query.Offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(query) + "." + nameof(query.Offset), "The offset must be equal or greater than zero.");
            }

            if (query.Page != null && query.Page != 1 /* ignore the default value for page */)
            {
                throw new NotSupportedException("Page and Offset cannot be used at the same time in a API request.");
            }

            source = source.Skip(query.Offset.Value);
        }

        var limit = query.Limit;

        if (query.Page != null)
        {
            if (query.Page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(query) + "." + nameof(query.Offset), "The page must be equal or greater than 1.");
            }

            if (limit == null)
            {
                throw new NotSupportedException("No limit defined. You cannot use page without a limit");
            }

            source = source.Skip((query.Page.Value - 1) * limit.Value);
        }

        if (limit != null)
        {
#pragma warning disable CS8629
            source = source.Take(limit.Value);
#pragma warning restore CS8629
        }

        return source;
    }
}
