
namespace BccCode.Linq.Client;
public static class IQueryableApiClientExtension
{
    // TODO this design leads to a lot of 'Provider' objects in memory. We should maybe redesign this so that the provider is instantiated once as long as the API client lives.

    /// <summary>
    /// Creates a Queryable object.
    /// </summary>
    /// <param name="apiClient">
    /// The API client.
    /// </param>
    /// <param name="path">
    /// The URL path to the API endpoint.
    /// </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> GetQueryable<T>(this IQueryableApiClient apiClient, string path, Action<IQueryableParameters>? parameters = null)
    {
        var provider = new ApiQueryProvider(apiClient, path, parameters);
        return new ApiQueryable<T>(provider);
    }
}
