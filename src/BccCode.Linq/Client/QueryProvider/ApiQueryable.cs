using System.Collections;
using System.Linq.Expressions;
using BccCode.Platform;

namespace BccCode.Linq.Client;

/// <summary>
/// IQueryable implementation that utilizes the query provider to execute queries against the source
/// </summary>
/// <typeparam name="T"></typeparam>
internal interface IApiQueryable
{
    /// <summary>
    /// Creates the <see cref="IApiCaller"/> for the data type of the queryable without
    /// having to know the generic data type.
    /// </summary>
    IApiCaller CreateApiCaller(IHttpApiQueryableClient apiClient, string path, Action<IQueryableParameters>? parametersCallback);
}

internal class ApiQueryable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>, IApiQueryable
{
    IApiCaller IApiQueryable.CreateApiCaller(IHttpApiQueryableClient apiClient, string path, Action<IQueryableParameters>? parametersCallback)
        => new QueryablePagedEnumerable<T>(apiClient, path, parametersCallback);

    public ApiQueryable(IQueryProvider provider, Expression? expression = default)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));

        expression ??= Expression.Constant(this);
        if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
        {
            throw new ArgumentOutOfRangeException(nameof(expression));
        }

        Expression = expression;
    }

    public Type ElementType => typeof(T);
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }

    public IEnumerator<T> GetEnumerator()
    {
        return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
    }

    #region IEnumerator

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
    }

    #endregion

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        if (Provider is not ApiQueryProvider apiQueryProvider)
            throw new NotSupportedException($"The Provider must be of type {typeof(ApiQueryProvider).FullName}");

        var enumerable = apiQueryProvider.ExecuteAsync<IAsyncEnumerable<T>>(Expression, cancellationToken);
        return enumerable.GetAsyncEnumerator(cancellationToken);
    }
}
