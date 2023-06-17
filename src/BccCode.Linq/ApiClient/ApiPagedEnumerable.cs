using System.Collections;

namespace BccCode.Linq.ApiClient;

/// <summary>
/// Internal. Makes it possible to easily getting the API request for a instance of <see cref="ApiPagedEnumerable{T}"/>
/// without knowing the generic data type.
/// </summary>
internal interface IApiCaller : IEnumerable
{
    public IApiRequest Request { get; }
}

/// <summary>
/// A class returning data from the API by requesting data per page. 
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ApiPagedEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IApiCaller
{
    /// <summary>
    /// The maximum number of rows per page allowed by the API.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public const int MaxRowsPerPage = 1000;

    private readonly IApiClient _apiClient;
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
    public IApiRequest Request { get; }

    public ApiPagedEnumerable(IApiClient apiClient, string path)
    {
        _apiClient = apiClient;
        _path = path;
        Request = apiClient.ConstructApiRequest(_path);
    }

    private IResultList<T>? RequestPage(int page)
    {
        Request.Page = page;
        return _apiClient.Get<IResultList<T>>(_path, Request);
    }

    private Task<IResultList<T>?> RequestPageAsync(int page, CancellationToken cancellationToken = new())
    {
        Request.Page = page;
        return _apiClient.GetAsync<IResultList<T>>(_path, Request, cancellationToken);
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        IResultList<T>? pageData;
        int page = 1;
        do
        {
            pageData = await RequestPageAsync(page++, cancellationToken);
            if (pageData == null)
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
            if (pageData == null)
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
