using BccCode.Linq.Server;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests;

public class InvertFilterTests
{
    [Fact]
    public void should_invert_a_field_filter_operator()
    {
        var filter = new Filter<TestClass>("{\"StrProp\":{\"_eq\":\"this\",\"_lt\":1,\"_lte\":2,\"_gt\":3,\"_gte\":4,\"_in\":[\"a\",\"b\",\"c\"],\"_null\":true,\"_starts_with\":\"is\",\"_ends_with\":\"is\",\"_contains\":\"is\",\"_between\":[10,20],\"_empty\":true,\"_submitted\":true,\"_regex\":\"^test$\"},\"AnotherStrProp\":{\"_neq\":\"that\",\"_nin\":[\"d\",\"e\",\"f\"],\"_nnull\":true,\"_nstarts_with\":\"at\",\"_nends_with\":\"at\",\"_ncontains\":\"at\",\"_nbetween\":[30,40],\"_nempty\":true}}");
        var expected = new Filter<TestClass>("{\"StrProp\":{\"_neq\":\"this\",\"_gte\":1,\"_gt\":2,\"_lte\":3,\"_lt\":4,\"_nin\":[\"a\",\"b\",\"c\"],\"_nnull\":true,\"_nstarts_with\":\"is\",\"_nends_with\":\"is\",\"_ncontains\":\"is\",\"_nbetween\":[10,20],\"_nempty\":true,\"_submitted\":false,\"_regex\":\"test$\"},\"AnotherStrProp\":{\"_eq\":\"that\",\"_in\":[\"d\",\"e\",\"f\"],\"_null\":true,\"_starts_with\":\"at\",\"_ends_with\":\"at\",\"_contains\":\"at\",\"_between\":[30,40],\"_empty\":true}}");
        var result = filter.GetInvertedFilter();

        Assert.True(
            (expected.Properties["StrProp"] as Filter).Properties.Keys.SequenceEqual(
                (result.Properties["StrProp"] as Filter).Properties.Keys));

        Assert.True(
            (expected.Properties["AnotherStrProp"] as Filter).Properties.Keys.SequenceEqual(
                (result.Properties["AnotherStrProp"] as Filter).Properties.Keys));
    }

    [Fact]
    public void should_invert_a_logical_filter_operators()
    {
        var filter = new Filter<TestClass>("{\"_and\":[{\"StrProp\":{\"_eq\":\"this\"}},{\"NumberIntergerProp\":{\"_neq\":\"125\"}}]}");
        var expected = new Filter<TestClass>("{\"_or\":[{\"StrProp\":{\"_neq\":\"this\"}},{\"NumberIntergerProp\":{\"_eq\":\"125\"}}]}");
        var result = filter.GetInvertedFilter();

        Assert.True(expected.Properties.Keys.SequenceEqual(result.Properties.Keys));

        Assert.True(
            ((expected.Properties["_or"] as Filter).Properties["StrProp"] as Filter).Properties.Keys.SequenceEqual(
                ((result.Properties["_or"] as Filter).Properties["StrProp"] as Filter).Properties.Keys));

        Assert.True(
            ((expected.Properties["_or"] as Filter).Properties["NumberIntergerProp"] as Filter).Properties.Keys.SequenceEqual(
                ((result.Properties["_or"] as Filter).Properties["NumberIntergerProp"] as Filter).Properties.Keys));
    }
}
