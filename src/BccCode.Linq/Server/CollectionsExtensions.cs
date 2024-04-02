using System.ComponentModel;
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

    /// <summary>
    /// Transforms the 'sort' HTML parameter into a structured data.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type to be sorted. Note that nested sorting is not supported.
    /// </typeparam>
    /// <param name="sort"></param>
    /// <returns>
    /// A enumerable returning a tuple per sorting. The tuple holds the property info
    /// of the property to sort and the sort direction.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Is thrown, when you try to sort nested properties.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Is thrown, when a sort property is not found in the type <typeparamref name="T"/>.
    /// </exception>
    internal static IEnumerable<(PropertyInfo, ListSortDirection)> GetSorting<T>(string sort)
        where T : class
    {
        foreach (var sortByField in sort.Split(','))
        {
            if (sortByField.Contains('.'))
                throw new NotSupportedException("Sorting on nested properties is not allowed");

            var propertyName = sortByField.Trim();

            if (string.IsNullOrEmpty(propertyName))
                // sort property not given, wrong call but optimistic handling, ignore it ...
                continue;

            ListSortDirection sortDirection;
            if (propertyName[0] == '-')
            {
                propertyName = propertyName[1..].Trim();

                if (string.IsNullOrEmpty(propertyName))
                    // sort property not given, wrong call but optimistic handling, ignore it ...
                    continue;

                // descending sorting
                sortDirection = ListSortDirection.Descending;
            }
            else
            {
                // ascending sorting
                sortDirection = ListSortDirection.Ascending;
            }

            // try find property
            var propertyInfo = typeof(T).GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.IgnoreCase);

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

                propertyInfo = typeof(T).GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            }

            if (propertyInfo == null)
            {
                throw new InvalidOperationException($"Could not find property for sort field {sortByField}");
            }

            yield return (propertyInfo, sortDirection);
        }
    }

    /// <summary>
    /// Applies query parameters to a <seealso cref="IQueryable"/> object.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type.
    /// </typeparam>
    /// <param name="source">
    /// The <seealso cref="IQueryable"/> object the query parameters shall be applied to.
    /// </param>
    /// <param name="query">
    /// The query parameters which shall be applied.
    /// </param>
    /// <param name="defaultSort">
    /// Optional. A default sorting which is applied when no sorting is applied via the query parameters (<paramref name="query"/>).
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Is thrown when:
    /// - <see cref="IQueryableParameters.Offset"/> is below zero.
    /// - <see cref="IQueryableParameters.Page"/> is below 1.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Is thrown when:
    /// - <see cref="IQueryableParameters.Offset"/> and <see cref="IQueryableParameters.Page"/> is used at the same time.
    /// - <see cref="IQueryableParameters.Page"/> is used and <see cref="IQueryableParameters.Limit"/> is not given.
    /// </exception>
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
            IOrderedQueryable<T>? orderedSource = null;

            foreach (var (propertyInfo, sortDirection) in GetSorting<T>(sort))
            {
                // build key selector
                var param = Expression.Parameter(type);
                var keySelector = Expression.Lambda(
                    Expression.Property(param, propertyInfo),
                    param);

                if (orderedSource == null)
                {
                    var orderByMethodInfo =
                        sortDirection == ListSortDirection.Descending
                            ? System.Linq.Internal.Queryable.OrderByDescendingMethodInfo
                            : System.Linq.Internal.Queryable.OrderByMethodInfo;

                    var method = orderByMethodInfo.MakeGenericMethod(type, propertyInfo.PropertyType);

                    orderedSource = (IOrderedQueryable<T>)source.Provider.CreateQuery(
                        Expression.Call(
                            null,
                            method,
                            source.Expression,
                            keySelector
                        ));
                }
                else
                {
                    var thenByMethodInfo =
                        sortDirection == ListSortDirection.Descending
                            ? System.Linq.Internal.Queryable.ThenByDescendingMethodInfo
                            : System.Linq.Internal.Queryable.ThenByMethodInfo;

                    var method = thenByMethodInfo.MakeGenericMethod(type, propertyInfo.PropertyType);

                    orderedSource = (IOrderedQueryable<T>)orderedSource.Provider.CreateQuery(
                        Expression.Call(
                            null,
                            method,
                            orderedSource.Expression,
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
