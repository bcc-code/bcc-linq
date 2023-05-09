using RuleFilterParser;

namespace RuleToLinqParser.Tests;

public class QueryableConverterTests
{
    [Fact]
    public void EqualQuery()
    {
        var json = DirectusFilterBuilder<Project>.Create()
            .Where(x => x.Status == true)
            .Serialize();

        Assert.Equal("{\"Status\":{\"_eq\":\"True\"}}", json);
    }

    [Fact]
    public void BetweenQuery()
    {
        var query = DirectusFilterBuilder<Project>.Create()
            .Where(x => x.Start >= new DateTime(2023, 5, 1) && x.Start <= new DateTime(2023, 5, 31));

        var json = query.Serialize();
        
        Assert.Equal("{\"Start\":{\"_gte\":\"2023-05-01T00:00:00.0000000\",\"_lte\":\"2023-05-31T00:00:00.0000000\"}}", json);
    }
}

public class Project
{
    public bool Status { get; set; }
    public DateTime Start { get; set; }
    public DateTime Modified { get; set; }
}