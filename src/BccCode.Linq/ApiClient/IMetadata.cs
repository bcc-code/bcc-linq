namespace BccCode.ApiClient;

public interface IMetadata : IReadOnlyDictionary<string, object>
{
    /// <summary>
    /// Returns the page limit of the request.
    /// </summary>
    public int Limit =>
        this.TryGetValue("limit", out var limit) ? (int)limit : default;

    /// <summary>
    /// Returns how many rows have been skipped from the start.
    /// </summary>
    public int Skipped =>
        this.TryGetValue("skipped", out var skipped) ? (int)skipped : default;

    /// <summary>
    /// Returns the number of rows, passed by the metadata property <c>"total"</c>.
    /// </summary>
    [Obsolete($"Please use either property {nameof(FilterCount)} instead.")]
    public int Total =>
        this.TryGetValue("total", out var total) ? (int)total : default;

    /// <summary>
    /// Returns the item count of the collection you're querying, taking the current filter/search parameters into account
    /// if given, otherwise <c>null</c>.
    /// </summary>
    public long? FilterCount =>
        this.TryGetValue("filter_count", out var filterCount) ? (long?)filterCount
        : this.TryGetValue("total", out var total) ? (int)total
        : null;

    /// <summary>
    /// Returns the total item count of the collection you're querying if given, otherwise <c>null</c>.
    /// </summary>
    public long? TotalCount =>
        this.TryGetValue("total_count", out var totalCount) ? (long?)totalCount : null;
}
