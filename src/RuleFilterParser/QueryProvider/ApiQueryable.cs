using System.Collections;
using System.Linq.Expressions;

namespace RuleFilterParser;

/// <summary>
/// IQueryable implementation that utilizes the query provider to execute queries against the source
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ApiQueryable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
{
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
