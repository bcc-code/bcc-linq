using BccCode.Linq.Server;

namespace BccCode.Linq.Tests;

public class QueryableConverterTests
{
    [Fact]
    public void EqualQuery()
    {
        var json = new DirectusFilterBuilder<Project>()
            .Where(x => x.Status == true)
            .Serialize();

        Assert.Equal("{\"Status\":{\"_eq\":\"True\"}}", json);
    }

    [Fact]
    public void NotEqualQuery()
    {
        var json = new DirectusFilterBuilder<Project>()
            .Where(x => x.Status != true)
            .Serialize();

        Assert.Equal("{\"Status\":{\"_neq\":\"True\"}}", json);
    }

    [Fact]
    public void BetweenTwoDatesQuery()
    {
        var query = new DirectusFilterBuilder<Project>()
            .Where(x => x.Start >= new DateTime(2023, 5, 1) && x.Start <= new DateTime(2023, 5, 31));

        var json = query.Serialize();

        Assert.Equal("{\"Start\":{\"_gte\":\"2023-05-01T00:00:00.0000000\",\"_lte\":\"2023-05-31T00:00:00.0000000\"}}", json);
    }

    [Fact]
    public void BetweenTwoDatesQueryMemberExpression()
    {
        var defaultFrom = DateTime.UtcNow.AddMonths(-6);
        var defaultTo = DateTime.UtcNow;
        var defaultFilterJson = new DirectusFilterBuilder<Project>()
            .Where(x => x.Start >= defaultFrom && x.Start <= defaultTo)
            .Serialize();

        Assert.NotEqual("{}", defaultFilterJson);
    }

    [Fact]
    public void GreaterThanOrEqual()
    {
        var json = new DirectusFilterBuilder<Project>()
            .Where(x => x.Start >= new DateTime(2023, 5, 1))
            .Serialize();

        Assert.Equal("{\"Start\":{\"_gte\":\"2023-05-01T00:00:00.0000000\"}}", json);
    }

    [Fact]
    public void GreaterThan()
    {
        var json = new DirectusFilterBuilder<Project>()
            .Where(x => x.Start > new DateTime(2023, 5, 1))
            .Serialize();

        Assert.Equal("{\"Start\":{\"_gt\":\"2023-05-01T00:00:00.0000000\"}}", json);
    }

    [Fact]
    public void LessThanOrEqual()
    {
        var json = new DirectusFilterBuilder<Project>()
            .Where(x => x.Start <= new DateTime(2023, 5, 1))
            .Serialize();

        Assert.Equal("{\"Start\":{\"_lte\":\"2023-05-01T00:00:00.0000000\"}}", json);
    }

    [Fact]
    public void LessThan()
    {
        var json = new DirectusFilterBuilder<Project>()
            .Where(x => x.Start < new DateTime(2023, 5, 1))
            .Serialize();

        Assert.Equal("{\"Start\":{\"_lt\":\"2023-05-01T00:00:00.0000000\"}}", json);
    }
}

public class Project
{
    public bool Status { get; set; }
    public DateTime Start { get; set; }
    public DateTime Modified { get; set; }
}
