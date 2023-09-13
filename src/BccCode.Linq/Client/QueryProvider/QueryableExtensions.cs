using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace BccCode.Linq.Client;

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
    public static IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, TProperty>> navigationPropertyPath)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (navigationPropertyPath == null)
            throw new ArgumentNullException(nameof(navigationPropertyPath));

        return
            new IncludableQueryable<TEntity, TProperty>(
                source.Provider is ApiQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: IncludeMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TProperty)),
                            arguments: new[] { source.Expression, Expression.Quote(navigationPropertyPath) }
                        ))
                    : source);
    }


    internal static readonly MethodInfo ThenIncludeAfterEnumerableMethodInfo
        = typeof(QueryableExtensions)
            .GetTypeInfo().GetDeclaredMethods(nameof(ThenInclude))
            .Where(mi => mi.GetGenericArguments().Length == 3)
            .Single(
                mi =>
                {
                    var typeInfo = mi.GetParameters()[0].ParameterType.GenericTypeArguments[1];
                    return typeInfo.IsGenericType
                        && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                });

    internal static readonly MethodInfo ThenIncludeAfterReferenceMethodInfo
        = typeof(QueryableExtensions)
            .GetTypeInfo().GetDeclaredMethods(nameof(ThenInclude))
            .Single(
                mi => mi.GetGenericArguments().Length == 3
                      && mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter);






    /// <summary>
    /// Specifies additional related data to be further included based on a related type that was just included.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <typeparam name="TPreviousProperty">The type of the entity that was just included.</typeparam>
    /// <typeparam name="TProperty">The type of the related entity to be included.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="navigationPropertyPath">
    /// A lambda expression representing the navigation property to be included (<c>t => t.Property1</c>).
    /// </param>
    /// <returns>A new query with the related data included.</returns>
    public static IIncludableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(
        this IIncludableQueryable<TEntity, TPreviousProperty> source,
        Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
        where TEntity : class
        => new IncludableQueryable<TEntity, TProperty>(
            // ReSharper disable once SuspiciousTypeConversion.Global
            source.Provider is ApiQueryProvider
                ? source.Provider.CreateQuery<TEntity>(
                    Expression.Call(
                        instance: null,
                        method: ThenIncludeAfterReferenceMethodInfo.MakeGenericMethod(
                            typeof(TEntity), typeof(TPreviousProperty), typeof(TProperty)),
                        arguments: new[] { source.Expression, Expression.Quote(navigationPropertyPath) }))
                : source);



    public static IIncludableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(
        this IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>> source,
        Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
        where TEntity : class
        => new IncludableQueryable<TEntity, TProperty>(
            source.Provider is ApiQueryProvider
                ? source.Provider.CreateQuery<TEntity>(
                    Expression.Call(
                        instance: null,
                        method: ThenIncludeAfterEnumerableMethodInfo.MakeGenericMethod(
                            typeof(TEntity), typeof(TPreviousProperty), typeof(TProperty)),
                        arguments: new[] { source.Expression, Expression.Quote(navigationPropertyPath) }))
                : source);

    private sealed class IncludableQueryable<TEntity, TProperty> : IIncludableQueryable<TEntity, TProperty>, IAsyncEnumerable<TEntity>
    {
        private readonly IQueryable<TEntity> _queryable;

        public IncludableQueryable(IQueryable<TEntity> queryable)
        {
            _queryable = queryable;
        }

        public Expression Expression
            => _queryable.Expression;

        public Type ElementType
            => _queryable.ElementType;

        public IQueryProvider Provider
            => _queryable.Provider;

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => ((IAsyncEnumerable<TEntity>)_queryable).GetAsyncEnumerator(cancellationToken);

        public IEnumerator<TEntity> GetEnumerator()
            => _queryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _queryable.GetEnumerator();
    }
    
    #endregion

    #region Search

    internal static readonly MethodInfo SearchMethodInfo
        = typeof(QueryableExtensions)
            .GetTypeInfo().GetDeclaredMethods(nameof(Search))
            .Single();

    public static IQueryable<TEntity> Search<TEntity>(this IQueryable<TEntity> source, string searchString)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (source.Provider is ApiQueryProvider)
        {
            return source.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    instance: null,
                    method: SearchMethodInfo.MakeGenericMethod(
                        typeof(TEntity)),
                    arguments: new []{source.Expression, Expression.Constant(searchString)}));
        }
 
        throw new NotSupportedException("Linq method Search was not replaced by the queryable provider. You can only use this method with a IQueryable<> object from an Bcc Code API. If you already use a IQueryable<> object from a Bcc Code API, try to call Search earlier in your Linq command chain.");
    }

    #endregion
}
