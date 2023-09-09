using BccCode.Linq.ApiClient;
using Microsoft.AspNetCore.Mvc;

namespace BccCode.Linq.Tests;

internal class ApiRequest : IQueryableParameters
{
    [FromQuery(Name = "fields")]
    public string? Fields { get; set; }
    [FromQuery(Name = "filter")]
    public string? Filter { get; set; }
    [FromQuery(Name = "search")]
    public string? Search { get; set; }
    [FromQuery(Name = "sort")]
    public string? Sort { get; set; }
    [FromQuery(Name = "limit")]
    public int? Limit { get; set; }
    [FromQuery(Name = "offset")]
    public int? Offset { get; set; }
    [FromQuery(Name = "page")]
    public int? Page { get; set; }
    [FromQuery(Name = "aggregate")]
    public string? Aggregate { get; set; }
    [FromQuery(Name = "groupBy")]
    public string? GroupBy { get; set; }
    [FromQuery(Name = "deep")]
    public string? Deep { get; set; }
    [FromQuery(Name = "alias")]
    public string? Alias { get; set; }
    [FromQuery(Name = "meta")]
    public string? Meta { get; set; }

    public IQueryableParameters Clone() => (ApiRequest)MemberwiseClone();
}
