﻿using BccCode.Linq.Server;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests;

public class RenameFieldInFilterTests
{
    [Fact]
    public void should_rename_field_filter()
    {
        var filter = new Filter<TestClass>("{\"test1\":{\"_eq\":\"lorem\"},\"test2\":{\"_eq\":\"ipsum\"}}");
        var expected = new Filter<TestClass>("{\"test100\":{\"_eq\":\"lorem\"},\"test2\":{\"_eq\":\"ipsum\"}}");
        filter.RenameFieldInFilter("test1", "test100");

        Assert.True(expected.Properties.Keys.SequenceEqual(filter.Properties.Keys));
    }

    [Fact]
    public void should_rename_field_filter_nested_in_another_field_filter()
    {
        var filter = new Filter<TestClass>("{\"Nested\":{\"test2\":{\"_eq\":\"ipsum\"}}}");
        var expected = new Filter<TestClass>("{\"Nested\":{\"NestedStrProp\":{\"_eq\":\"ipsum\"}}}");
        filter.RenameFieldInFilter("test2", "NestedStrProp", "Nested");

        Assert.True(
            (expected.Properties["Nested"] as Filter).Properties.Keys.SequenceEqual(
                (filter.Properties["Nested"] as Filter).Properties.Keys));
    }

    [Fact]
    public void should_rename_field_filter_nested_in_logical_operator()
    {
        var filter = new Filter<TestClass>("{\"_and\":[{\"test2\":{\"_eq\":\"xyz\"}}]}");
        var expected = new Filter<TestClass>("{\"_and\":[{\"test100\":{\"_eq\":\"xyz\"}}]}");
        filter.RenameFieldInFilter("test2", "test100", "_and");

        Assert.True(
            (expected.Properties["_and"] as Filter<TestClass>).Properties.Keys.SequenceEqual(
                (filter.Properties["_and"] as Filter<TestClass>).Properties.Keys));

    }
}
