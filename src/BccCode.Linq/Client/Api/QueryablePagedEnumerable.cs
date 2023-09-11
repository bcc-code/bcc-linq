using System.Collections;
using System.Threading;


namespace BccCode.Linq.Client;

/// <summary>
/// Internal. Makes it possible to easily getting the API request for a instance of <see cref="QueryablePagedEnumerable{T}"/>
/// without knowing the generic data type.
/// </summary>
internal interface IApiCaller : IEnumerable
{
    public IQueryableParameters QueryParameters { get; }
}

/// <summary>
/// A class returning data from the API by requesting data per page. 
/// </summary>
/// <typeparam name="T"></typeparam>
internal partial class QueryablePagedEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IApiCaller
{
    /// <summary>
    /// The maximum number of rows per page allowed by the API.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public const int MaxRowsPerPage = 1000;

    private readonly IQueryableApiClient _apiClient;
    private readonly string _path;
    private int _queryBatchSize = 100;

    /// <summary>
    /// Set the number of data models to be retrieved per API call. 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int QueryBatchSize
    {
        get => _queryBatchSize;
        set
        {
            if (value <= 0 || value > MaxRowsPerPage)
                throw new ArgumentOutOfRangeException($"The rows per page can be between 1 and {MaxRowsPerPage}");

            _queryBatchSize = value;
        }
    }

    /// <summary>
    /// The API request which will be passed to a single API call. 
    /// </summary>
    public IQueryableParameters QueryParameters { get; }

    public QueryablePagedEnumerable(IQueryableApiClient apiClient, string path, Action<IQueryableParameters>? parametersCallback)
    {
        _apiClient = apiClient;
        _path = path;
        QueryBatchSize = apiClient.QueryBatchSize ?? QueryBatchSize;
        QueryParameters = apiClient.CreateQueryableParameters(_path);
        parametersCallback?.Invoke(QueryParameters);
    }

    private ResultList<T>? RequestPage(IQueryableParameters parameters)
    {
        return _apiClient.FetchPage<ResultList<T>>(_path, parameters);
    }

    private Task<ResultList<T>?> RequestPageAsync(IQueryableParameters parameters, CancellationToken cancellationToken = default)
    {
        return _apiClient.FetchPageAsync<ResultList<T>>(_path, parameters, cancellationToken);
    }


    public async Task<IResultList<T>?> FetchAsync(CancellationToken cancellationToken = default)
    {
        var resultList = new ResultList<T>();
        resultList.Data = new List<T>();
        ResultList<T>? pageData;
        var state = QueryablePagedEnumerableState.Create(QueryParameters, QueryBatchSize);
        do
        {
            pageData = await RequestPageAsync(state.PageParameters, cancellationToken);
            if (pageData == null)
                break;

            if (state.TotalRequested == 0) //First page
            {
                resultList.Meta = new Metadata(pageData.Meta);
            }

            resultList.AddData(pageData.Data);

        } while (state.NextPage(pageData.Data?.Count ?? 0));

        return resultList.ToImmutableResultList();
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        ResultList<T>? pageData;
        var state = QueryablePagedEnumerableState.Create(QueryParameters, QueryBatchSize);
        do
        {
            pageData = await RequestPageAsync(state.PageParameters, cancellationToken);
            if (pageData?.Data == null)
                yield break;

            foreach (var row in pageData.Data)
            {
                yield return row;
            }

        } while (state.NextPage(pageData.Data?.Count ?? 0));
    }

    public IEnumerator<T> GetEnumerator()
    {
        ResultList<T>? pageData;
        var state = QueryablePagedEnumerableState.Create(QueryParameters, QueryBatchSize);
        do
        {
            pageData = RequestPage(state.PageParameters);
            if (pageData?.Data == null)
                yield break;

            foreach (var row in pageData.Data)
            {
                yield return row;
            }

        } while (state.NextPage(pageData.Data?.Count ?? 0));
    }

    #region IEnumerator

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}
