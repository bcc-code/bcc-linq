namespace RuleFilterParser;

public interface IApiClient
{
    /// <summary>
    /// Returns a list of entries 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="request"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    TResult? Get<TResult>(string path, IApiRequest request)
        where TResult : class;

    Task<TResult?> GetAsync<TResult>(string path, IApiRequest request, CancellationToken cancellationToken = default)
        where TResult : class;
}