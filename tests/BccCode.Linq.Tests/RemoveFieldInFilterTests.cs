using BccCode.Linq.Server;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests;

public class RemoveFieldFromFilterTests
{
    [Fact]
    public void should_remove_field_from_filter()
    {
        var filter = new Filter<TestClass>("{\"test1\":{\"_eq\":\"lorem\"},\"test2\":{\"_eq\":\"ipsum\"}}");
        var expected = new Filter<TestClass>("{\"test2\":{\"_eq\":\"ipsum\"}}");
        filter.RemoveFieldFromFilter("test1");

        Assert.True(expected.Properties.Keys.SequenceEqual(filter.Properties.Keys));
    }

    [Fact]
    public void should_remove_field_filter_nested_in_another_field_filter()
    {
        var filter =
            new Filter<TestClass>(
                "{\"Nested\":{\"deeper\":{\"_eq\":\"lorem\"},\"leave\":{\"_eq\":\"x\"}},\"NumberIntergerProp\":{\"_eq\":\"125\"}}");
        var expected = new Filter<TestClass>("{\"Nested\":{\"leave\":{\"_eq\":\"x\"}},\"NumberIntergerProp\":{\"_eq\":\"125\"}}");
        filter.RemoveFieldFromFilter("deeper", "Nested");

        Assert.True(
            (expected.Properties["Nested"] as Filter).Properties.Keys.SequenceEqual(
                (filter.Properties["Nested"] as Filter).Properties.Keys));
    }

    [Fact]
    public void should_remove_field_filter_nested_in_logical_operator()
    {
        var filter = new Filter<TestClass>("{\"_and\":[{\"test1\":{\"_eq\":\"lorem\"}},{\"test2\":{\"_eq\":\"ipsum\"}}]}");
        var expected = new Filter<TestClass>("{\"_and\":[{\"test2\":{\"_eq\":\"ipsum\"}}]}");
        filter.RemoveFieldFromFilter("test1", "_and");

        Assert.True(
            (expected.Properties["_and"] as Filter<TestClass>).Properties.Keys.SequenceEqual(
                (filter.Properties["_and"] as Filter<TestClass>).Properties.Keys));
    }
}
