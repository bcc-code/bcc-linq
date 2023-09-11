
namespace BccCode.Linq.Client;

public interface IQueryableApiClient
{
    /// <summary>
    /// Number of rows to request from server when making paged requests.
    /// </summary>
    int? QueryBatchSize { get; }

    /// <summary>
    /// Creates an API request instance for a specific API endpoint
    /// which must implement <see cref="IQueryableApiClient"/>. 
    /// </summary>
    /// <param name="path">
    /// URL path of the API endpoint.
    /// </param>
    /// <returns>
    /// A new API request model.
    /// </returns>
    IQueryableParameters CreateQueryableParameters(string path);

    /// <summary>
    /// Returns a list of entries.
    /// </summary>
    /// <param name="path">
    /// URL path of the API endpoint.
    /// </param>
    /// <param name="pageQueryParameters">
    /// The API request parameters.
    /// </param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    TResult? FetchPage<TResult>(string path, IQueryableParameters pageQueryParameters)
        where TResult : class;

    /// <summary>
    /// Returns the result of a single page request.
    /// </summary>
    /// <param name="path">
    /// URL path of the API endpoint.
    /// </param>
    /// <param name="pageQueryParameters">
    /// The API request parameters.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T?> FetchPageAsync<T>(string path, IQueryableParameters pageQueryParameters, CancellationToken cancellationToken = default)
        where T : class;
}
