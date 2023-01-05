using RuleFilterParser;

namespace RuleToLinqParser.Tests;

public class RemoveFieldFromFilterTests
{
    [Fact]
    public void should_remove_field_from_filter()
    {
        var filter = new Filter("{\"test1\":{\"_eq\":\"lorem\"},\"test2\":{\"_eq\":\"ipsum\"}}");
        var expected = new Filter("{\"test2\":{\"_eq\":\"ipsum\"}}");
        filter.RemoveFieldFromFilter("test1");

        Assert.True(expected.Properties.Keys.SequenceEqual(filter.Properties.Keys));
    }

    [Fact]
    public void should_remove_field_filter_nested_in_another_field_filter()
    {
        var filter =
            new Filter(
                "{\"test1\":{\"deeper\":{\"_eq\":\"lorem\"},\"leave\":{\"_eq\":\"x\"}},\"test2\":{\"_eq\":\"ipsum\"}}");
        var expected = new Filter("{\"test1\":{\"leave\":{\"_eq\":\"x\"}},\"test2\":{\"_eq\":\"ipsum\"}}");
        filter.RemoveFieldFromFilter("deeper");

        Assert.True(
            (expected.Properties["test1"] as Filter).Properties.Keys.SequenceEqual(
                (filter.Properties["test1"] as Filter).Properties.Keys));
    }

    [Fact]
    public void should_remove_field_filter_nested_in_logical_operator()
    {
        var filter = new Filter("{\"_and\":[{\"test1\":{\"_eq\":\"lorem\"}},{\"test2\":{\"_eq\":\"ipsum\"}}]}");
        var expected = new Filter("{\"_and\":[{\"test2\":{\"_eq\":\"ipsum\"}}]}");
        filter.RemoveFieldFromFilter("test1");

        Assert.True(
            (expected.Properties["_and"] as Filter).Properties.Keys.SequenceEqual(
                (filter.Properties["_and"] as Filter).Properties.Keys));
    }
}