using System.Collections;
using System.Diagnostics;
using RuleFilterParser;

namespace RuleToLinqParser.Tests;

/// <summary>
/// An mockup API client which always returns an empty object,
/// but provides the endpoint and request as properties for testing.
///
/// This class is build for testing the QueryProvider 
/// </summary>
internal class ApiClientMockup : IApiClient
{
    private class ResultList<T> : IResultList<T>
    {
        public ResultList(List<T> data, Dictionary<string, object>? meta)
        {
            Data = data;
            Meta = meta ?? new Dictionary<string, object>();
        }
        
        public List<T> Data { get; }
        public Dictionary<string, object> Meta { get; }

        #region IResultList<T>

        IReadOnlyList<T> IResultList<T>.Data => Data;
        IReadOnlyDictionary<string, object> IMeta.Meta => Meta;

        #endregion
    }
    
    private readonly Dictionary<Type, IList> _inMemoryData = new();
    
    public string? LastEndpoint { get; set; }
    public IApiRequest? LastRequest { get; set; }
    
    public void RegisterData(Type type, IList enumerable)
    {
        if (_inMemoryData.ContainsKey(type))
            throw new ArgumentException($"The data of type {type.FullName} has already been registered", nameof(type));
        
        _inMemoryData.Add(type, enumerable);
    }
    
    #region IApiClient

    public TResult? Get<TResult>(string endpoint, IApiRequest request)
        where TResult : class
    {
        LastEndpoint = endpoint;
        LastRequest = request;

        var resultType = typeof(TResult);
        
        Debug.Assert(resultType.IsGenericType);
        if (resultType.GetGenericTypeDefinition() == typeof(IResultList<>))
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

    public Task<TResult?> GetAsync<TResult>(string endpoint, IApiRequest request, CancellationToken cancellationToken = default)
        where TResult : class
    {
        LastEndpoint = endpoint;
        LastRequest = request;

        return Task.FromResult<TResult?>(null);
    }

    #endregion
}