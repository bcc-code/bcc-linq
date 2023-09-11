
namespace BccCode.Linq.Client;

/// <summary>
/// Defines method to execute queries asynchronously that are described by an IQueryable object.
/// </summary>
/// <remarks>
/// This is a clone of
/// <a href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.iasyncqueryprovider?view=efcore-7.0">
/// Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider</a>
/// </remarks>
public interface IAsyncQueryProvider : IQueryProvider
{
    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default);
}
