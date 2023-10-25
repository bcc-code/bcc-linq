using System.Collections;
using System.Diagnostics;
using BccCode.Linq.Client;
using BccCode.Platform;

namespace BccCode.Linq.Tests;

/// <summary>
/// An mockup API client which always returns an empty object,
/// but provides the endpoint and request as properties for testing.
///
/// This class is build for testing the QueryProvider 
/// </summary>
public class ApiClientMockupBase : IHttpApiQueryableClient
{
    private readonly Dictionary<Type, IList> _inMemoryData = new();

    public string? PageEndpoint { get; set; }
    public IQueryableParameters? ClientQuery { get; set; }
    public IQueryableParameters? PageQuery { get; set; }

    public int? QueryBatchSize => 100;

    public void RegisterData(Type type, IEnumerable enumerable)
    {
        if (_inMemoryData.ContainsKey(type))
            throw new ArgumentException($"The data of type {type.FullName} has already been registered", nameof(type));

        var listType = typeof(List<>).MakeGenericType(type);
        var inMemoryList = Activator.CreateInstance(listType, enumerable);
#pragma warning disable CS8604
#pragma warning disable CS8600
        _inMemoryData.Add(type, (IList)inMemoryList);
#pragma warning restore CS8600
#pragma warning restore CS8604
    }

    #region IApiClient

    public TResult? GetResult<TResult>(string endpoint, IQueryableParameters query)
        where TResult : class
    {
        PageEndpoint = PageEndpoint ?? endpoint;
        PageQuery = query;

        var resultType = typeof(TResult);

        Debug.Assert(resultType.IsGenericType);
        if (resultType.GetGenericTypeDefinition() == typeof(ResultList<>) ||
            resultType.GetGenericTypeDefinition() == typeof(IResultList<>))
        {
            var type = resultType.GenericTypeArguments[0];
            if (_inMemoryData.TryGetValue(type, out var inMemory))
            {
                var resultListType = typeof(ResultList<>).MakeGenericType(type);
#pragma warning disable CS8600
                return (TResult)Activator.CreateInstance(resultListType, inMemory, null);
#pragma warning restore CS8600
            }

            throw new Exception($"No In-Memory data registered in ApiClientMockup with type {resultType.GenericTypeArguments[0].FullName}");
        }

        throw new Exception("Invalid call of method No In-Memory data registered in ApiClientMockup");
    }

    public Task<TResult?> GetResultAsync<TResult>(string endpoint, IQueryableParameters request, CancellationToken cancellationToken = default)
        where TResult : class
    {
        PageEndpoint = endpoint;
        PageQuery = request;

        return Task.FromResult(
            GetResult<TResult>(endpoint, request)
        );
    }

    IQueryableParameters IHttpApiQueryableClient.CreateQueryableParameters(string path)
    {
        // NOTE: The Mockup API does just use a single ApiRequest class.
        //       A real API client might use different request model classes for different endpoints.
        return new ApiRequest();
    }

    #endregion
}
