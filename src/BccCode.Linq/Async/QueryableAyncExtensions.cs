namespace BccCode.Linq.Async;

/// <summary>
/// Adds extension methods to run async. operations against an instance of <see cref="IQueryable{T}"/>. 
/// </summary>
public static class QueryableAsyncExtensions
{
    /// <summary>
    /// Processes the expression tree behind <paramref name="queryable"/> object and returns
    /// a <see cref="IAsyncEnumerator{T}"/> object for further async calls. 
    /// </summary>
    /// <remarks>
    /// The whole expression tree is parsed. When the expression tree contains logic which cannot be
    /// handled by the <see cref="IAsyncQueryProvider" /> (e.g. you pass a join to a API endpoint provider),
    /// an exception <see cref="InvalidOperationException"/> is thrown. Try to simplify your query or use
    /// <see cref="ToListAsync{TSource}"/> prior calling advanced Linq logic which the provider cannot handle.
    /// </remarks>
    /// <param name="queryable">
    /// The queryable object which shall be casted. The provider of the queryable object must implement
    /// <see cref="IAsyncQueryProvider" />.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="queryable"/>.</typeparam>
    /// <returns></returns>
    /// <example>
    /// Example using <b>AsAsyncEnumerable</b>:
    /// <code>
    /// await foreach(var item in query.AsAsyncEnumerable())
    /// {
    ///     // Processing of item here ...
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// Is thrown when <paramref name="queryable"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Is thrown when the Provider of <paramref name="queryable"/> does not implement <see cref="IAsyncQueryProvider"/>.
    /// </exception>
    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IQueryable<TSource> queryable, CancellationToken cancellationToken = default)
    {
        if (queryable == null)
            throw new ArgumentNullException(nameof(queryable));

        if (queryable.Provider is not IAsyncQueryProvider asyncQueryProvider)
            throw new ArgumentException(
                $"The Provider behind the query must implement {typeof(IAsyncQueryProvider).FullName}",
                nameof(queryable));

        return asyncQueryProvider.ExecuteAsync<IAsyncEnumerable<TSource>>(queryable.Expression, cancellationToken);
    }

    /// <summary>
    /// Creates a <see cref="List{T}"/> from an <see cref="IQueryable{T}"/> which has a Provider implementing <see cref="IAsyncQueryProvider"/>.
    /// </summary>
    /// <param name="source">
    /// The <see cref="IQueryable{T}"/> to create a <see cref="List{T}"/> from.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <returns>
    /// A <see cref="List{T}"/> that contains elements from the input sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var list = new List<TSource>();

        await foreach (var item in source.AsAsyncEnumerable(cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// Returns the first element of a sequence.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The source sequence is empty.
    /// </exception>
    public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var asyncEnumerable = source.Take(1).AsAsyncEnumerable(cancellationToken);

        var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);

        if (await enumerator.MoveNextAsync())
        {
            return enumerator.Current;
        }
        else
        {
            throw new InvalidOperationException("The source sequence is empty.");
        }
    }

    /// <summary>
    /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
    /// </summary>
    /// <param name="source">The <see cref="IQueryable{T}"/> to return the first element of.</param>
    /// <param name="cancellationToken">The type of the elements of <paramref name="source"/></param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <returns>
    /// <c>default</c>(<typeparamref name="TSource"/>) if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public static async Task<TSource?> FirstOrDefault<TSource>(this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var asyncEnumerable = source.Take(1).AsAsyncEnumerable(cancellationToken);

        var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);

        if (await enumerator.MoveNextAsync())
        {
            return enumerator.Current;
        }

        return default(TSource);
    }
}
