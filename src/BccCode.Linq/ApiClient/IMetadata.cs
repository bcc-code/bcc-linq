namespace BccCode.Linq.ApiClient;

public interface IMetadata : IReadOnlyDictionary<string, object>
{
    /// <summary>
    /// Returns the page limit of the request.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Returns how many rows have been skipped from the start.
    /// </summary>
    public int Skipped { get; }

    /// <summary>
    /// Returns the number of rows, passed by the metadata property <c>"total"</c>.
    /// </summary>
    [Obsolete($"Please use property {nameof(FilterCount)} instead.")]
    public long Total { get; }

    /// <summary>
    /// Returns the item count of the collection you're querying, taking the current filter/search parameters into account
    /// if given, otherwise <c>null</c>.
    /// </summary>
    public long? FilterCount { get; }

    /// <summary>
    /// Returns the total item count of the collection you're querying if given, otherwise <c>null</c>.
    /// </summary>
    public long? TotalCount { get; }
}
