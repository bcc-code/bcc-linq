
using System.Diagnostics.CodeAnalysis;
using BccCode.Platform;

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
    [RequiresDynamicCode("LINQ queries are translated at runtime and can need generic instantiations over the element and result types of the query. Value-type instantiations may not be available in Native AOT.")]
    [RequiresUnreferencedCode("LINQ queries are translated at runtime. Executing them can rebind IQueryable methods to IEnumerable methods, which can be trimmed.")]
    public static IQueryable<T> GetQueryable<T>(this IHttpApiQueryableClient apiClient, string path, Action<IQueryableParameters>? parameters = null)
    {
        var provider = new ApiQueryProvider(apiClient, path, parameters);
        return new ApiQueryable<T>(provider);
    }

}
