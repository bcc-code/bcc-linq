namespace BccCode.ApiClient;

public interface IApiClient
{
    /// <summary>
    /// Creates an API request instance for a specific API endpoint
    /// which must implement <see cref="IApiClient"/>. 
    /// </summary>
    /// <param name="path">
    /// URL path of the API endpoint.
    /// </param>
    /// <returns>
    /// A new API request model.
    /// </returns>
    IApiQueryParameters ConstructApiRequest(string path);

    /// <summary>
    /// Returns a list of entries.
    /// </summary>
    /// <param name="path">
    /// URL path of the API endpoint.
    /// </param>
    /// <param name="query">
    /// The API request parameters.
    /// </param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    TResult? Get<TResult>(string path, IApiQueryParameters query)
        where TResult : class;

    /// <summary>
    /// Returns the result of a single page request.
    /// </summary>
    /// <param name="path">
    /// URL path of the API endpoint.
    /// </param>
    /// <param name="query">
    /// The API request parameters.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string path, IApiQueryParameters query, CancellationToken cancellationToken = default)
        where T : class;
}
