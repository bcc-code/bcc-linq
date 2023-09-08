using System.Collections;
using System.Threading;
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

    private ResultList<T>? RequestPage()
    {
        return _apiClient.Query<ResultList<T>>(_path, QueryParameters);
    }

    private Task<ResultList<T>?> RequestPageAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.QueryAsync<ResultList<T>>(_path, QueryParameters, cancellationToken);
    }

    public async Task<IResultList<T>?> FetchAsync(CancellationToken cancellationToken = default)
    {
        var resultList = new ResultList<T>();
        resultList.Data = new List<T>();
        ResultList<T>? pageData;
        int? initialLimit = QueryParameters.Limit;
        int? initialOffset = QueryParameters.Offset;
        int? initialPage = QueryParameters.Page;
        var hasLimit = initialLimit.HasValue && initialLimit > 0;
        var usePaging = initialPage.HasValue && initialPage > 0;

        int pageSize = usePaging && hasLimit ? initialLimit!.Value : 
                       (hasLimit && initialLimit!.Value < RowsPerPage ? initialLimit!.Value : RowsPerPage);
        int offset = usePaging ? (initialPage!.Value-1) * pageSize : (initialOffset ?? 0);

        // Modify query paramter values for paging
        QueryParameters.Page = null;
        QueryParameters.Offset = offset > 0 ? offset : null;
        QueryParameters.Limit = pageSize;
        
        int totalRequested = 0;      
        do
        {
            if (hasLimit && !usePaging && initialLimit!.Value < (totalRequested + pageSize))
            {
                pageSize = initialLimit!.Value - totalRequested;
                QueryParameters.Limit = pageSize;
            }
            pageData = await RequestPageAsync(cancellationToken);            
            if (pageData == null)
                break;

            if (totalRequested == 0) //First page
            {
                resultList.Meta = new Metadata(pageData.Meta);
            }

            resultList.AddData(pageData.Data);

            QueryParameters.Offset = (QueryParameters.Offset ?? 0) + pageSize;
            totalRequested += pageSize;
        } while ((pageData.Data?.Count ?? 0) == pageSize && (!hasLimit || totalRequested < initialLimit!.Value));

        // Restore query paramter values
        QueryParameters.Page = initialPage;
        QueryParameters.Offset = initialOffset;
        QueryParameters.Limit = initialLimit;

        if (resultList.Data.Count == 0)
            return null;


        return resultList.ToImmutableResultList();
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        ResultList<T>? pageData;
        int? initialLimit = QueryParameters.Limit;
        int? initialOffset = QueryParameters.Offset;
        int? initialPage = QueryParameters.Page;
        var hasLimit = initialLimit.HasValue && initialLimit > 0;
        var usePaging = initialPage.HasValue && initialPage > 0;

        int pageSize = usePaging && hasLimit ? initialLimit!.Value :
                       (hasLimit && initialLimit!.Value < RowsPerPage ? initialLimit!.Value : RowsPerPage);
        int offset = usePaging ? (initialPage!.Value - 1) * pageSize : (initialOffset ?? 0);

        // Modify query paramter values for paging
        QueryParameters.Page = null;
        QueryParameters.Offset = offset > 0 ? offset : null;
        QueryParameters.Limit = pageSize;

        int totalRequested = 0;
        do
        {
            if (hasLimit && !usePaging && initialLimit!.Value < (totalRequested + pageSize))
            {
                pageSize = initialLimit!.Value - totalRequested;
                QueryParameters.Limit = pageSize;
            }
            pageData = await RequestPageAsync(cancellationToken);
            if (pageData?.Data == null)
                yield break;

            foreach (var row in pageData.Data)
            {
                yield return row;
            }

            QueryParameters.Offset = (QueryParameters.Offset ?? 0) + pageSize;
            totalRequested += pageSize;
        } while ((pageData.Data?.Count ?? 0) == pageSize && (!hasLimit || totalRequested < initialLimit!.Value));

        // Restore query paramter values
        QueryParameters.Page = initialPage;
        QueryParameters.Offset = initialOffset;
        QueryParameters.Limit = initialLimit;
    }

    public IEnumerator<T> GetEnumerator()
    {
        ResultList<T>? pageData;
        int? initialLimit = QueryParameters.Limit;
        int? initialOffset = QueryParameters.Offset;
        int? initialPage = QueryParameters.Page;
        var hasLimit = initialLimit.HasValue && initialLimit > 0;
        var usePaging = initialPage.HasValue && initialPage > 0;

        int pageSize = usePaging && hasLimit ? initialLimit!.Value :
                       (hasLimit && initialLimit!.Value < RowsPerPage ? initialLimit!.Value : RowsPerPage);
        int offset = usePaging ? (initialPage!.Value - 1) * pageSize : (initialOffset ?? 0);

        // Modify query paramter values for paging
        QueryParameters.Page = null;
        QueryParameters.Offset = offset > 0 ? offset : null;
        QueryParameters.Limit = pageSize;

        int totalRequested = 0;
        do
        {
            if (hasLimit && !usePaging && initialLimit!.Value < (totalRequested + pageSize))
            {
                pageSize = initialLimit!.Value - totalRequested;
                QueryParameters.Limit = pageSize;
            }
            pageData = RequestPage();
            if (pageData?.Data == null)
                yield break;

            foreach (var row in pageData.Data)
            {
                yield return row;
            }

            QueryParameters.Offset = (QueryParameters.Offset ?? 0) + pageSize;
            totalRequested += pageSize;
        } while ((pageData.Data?.Count ?? 0) == pageSize && (!hasLimit || totalRequested < initialLimit!.Value));

        // Restore query paramter values
        QueryParameters.Page = initialPage;
        QueryParameters.Offset = initialOffset;
        QueryParameters.Limit = initialLimit;
    }

    #region IEnumerator

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}
