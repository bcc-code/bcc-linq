using System.Linq.Expressions;
using System.Reflection;

namespace BccCode.Linq;

/// <summary>
/// Adds extension methods for <see cref="IQueryable{T}"/> to specify advanced behavior not covered
/// by default C# Linq.
/// </summary>
public static class QueryableExtensions
{
    #region Include
    
    internal static readonly MethodInfo IncludeMethodInfo
        = typeof(QueryableExtensions)
            .GetTypeInfo().GetDeclaredMethods(nameof(Include))
            .Single(
                mi =>
                    mi.GetGenericArguments().Length == 2
                    && mi.GetParameters().Any(
                        pi => pi.Name == "navigationPropertyPath" && pi.ParameterType != typeof(string)));
    
    /// <summary>
    /// Specifies related entities to include in the query results. The navigation property to be included is specified starting with the
    /// type of entity being queried (<typeparamref name="TEntity" />). <!--If you wish to include additional types based on the navigation
    /// properties of the type being included, then chain a call to
    /// <see
    ///     cref="ThenInclude{TEntity, TPreviousProperty, TProperty}(IIncludableQueryable{TEntity, IEnumerable{TPreviousProperty}}, Expression{Func{TPreviousProperty, TProperty}})" />
    /// after this call.-->
    /// </summary>
    /// <param name="source">The source query.</param>
    /// <param name="navigationPropertyPath">
    /// A lambda expression representing the navigation property to be included (<c>t => t.Property1</c>).
    /// </param>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <typeparam name="TProperty">The type of the related entity to be included.</typeparam>
    /// <returns>
    /// A new query with the related data included.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="source" /> or <paramref name="navigationPropertyPath" /> is <see langword="null" />.
    /// </exception>
    public static IQueryable<TEntity> Include<TEntity, TProperty>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, TProperty>> navigationPropertyPath)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (navigationPropertyPath == null)
            throw new ArgumentNullException(nameof(navigationPropertyPath)); 

        return source is ApiQueryable<TEntity>
            ? source.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    instance: null,
                    method: IncludeMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TProperty)),
                    arguments: new[] { source.Expression, Expression.Quote(navigationPropertyPath) }
                ))
            : source;
    }
    
    #endregion
}
