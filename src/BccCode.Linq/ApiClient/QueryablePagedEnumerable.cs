using System.Collections;
using BccCode.Linq.ApiClient.Immutable;

namespace BccCode.Linq.ApiClient;

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
internal class QueryablePagedEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IApiCaller
{
    /// <summary>
    /// The maximum number of rows per page allowed by the API.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public const int MaxRowsPerPage = 1000;

    private readonly IQueryableApiClient _apiClient;
    private readonly string _path;
    private int _rowsPerPage = 100;

    /// <summary>
    /// Set the number of data models to be retrieved per API call. 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int RowsPerPage
    {
        get => _rowsPerPage;
        set
        {
            if (value <= 0 || value > MaxRowsPerPage)
                throw new ArgumentOutOfRangeException($"The rows per page can be between 1 and {MaxRowsPerPage}");

            _rowsPerPage = value;
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
        QueryParameters = apiClient.ConstructQueryableParameters(_path);
        parametersCallback?.Invoke(QueryParameters);        
    }

    private IResultList<T>? RequestPage(int page)
    {
        QueryParameters.Page = page;
        return _apiClient.Query<IResultList<T>>(_path, QueryParameters);
    }

    private Task<IResultList<T>?> RequestPageAsync(int page, CancellationToken cancellationToken = default)
    {
        QueryParameters.Page = page;
        return _apiClient.QueryAsync<IResultList<T>>(_path, QueryParameters, cancellationToken);
    }

    public async Task<IResultList<T>?> FetchAsync(CancellationToken cancellationToken = default)
    {
        var resultList = new ResultList<T>();
        resultList.Data = new List<T>();
        IResultList<T>? pageData;
        int page = 1;
        do
        {
            pageData = await RequestPageAsync(page, cancellationToken);
            if (pageData == null)
                break;

            if (page == 1)
            {
                resultList.Meta = new Metadata(pageData.Meta);
            }

            resultList.AddData(pageData.Data);

            page++;
        } while ((pageData.Data?.Count ?? 0) == RowsPerPage);

        if (resultList.Data.Count == 0)
            return null;


        return resultList.ToImmutableResultList();
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        IResultList<T>? pageData;
        int page = 1;
        do
        {
            pageData = await RequestPageAsync(page++, cancellationToken);

            if (pageData?.Data == null)
                yield break;

            foreach (var row in pageData.Data)
            {
                yield return row;
            }
        } while (pageData.Data.Count == RowsPerPage);
    }

    public IEnumerator<T> GetEnumerator()
    {
        IResultList<T>? pageData;
        int page = 1;
        do
        {
            pageData = RequestPage(page++);
            if (pageData?.Data == null)
                yield break;

            foreach (var row in pageData.Data)
            {
                yield return row;
            }
        } while (pageData.Data.Count == RowsPerPage);
    }

    #region IEnumerator

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}
