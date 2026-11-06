using System.Text.Json;
using BccCode.Linq.Server;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests;

public class FilterJsonTests
{
    public enum Color
    {
        Red = 1,
        Green = 2,
    }

    public class EnumHolder
    {
        public Color Color { get; set; }
    }

    [Fact]
    public void DeserializeJsonRule_returns_json_elements()
    {
        var result = FilterDeserializationHelpers.DeserializeJsonRule("{\"a\":{\"_eq\":1},\"b\":[1,2]}");

        Assert.IsType<JsonElement>(result["a"]);
        Assert.Equal(JsonValueKind.Object, ((JsonElement)result["a"]).ValueKind);
        Assert.Equal(JsonValueKind.Array, ((JsonElement)result["b"]).ValueKind);
    }

    [Theory]
    [InlineData("{ invalid")]
    [InlineData("null")]
    [InlineData("[1]")]
    public void DeserializeJsonRule_throws_argument_exception_for_invalid_json(string json)
    {
        var ex = Assert.Throws<ArgumentException>(() => FilterDeserializationHelpers.DeserializeJsonRule(json));

        Assert.StartsWith("JSON filter rule is invalid.", ex.Message);
    }

    [Fact]
    public void Invalid_json_has_system_text_json_exception_as_inner_exception()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Filter<TestClass>("{ invalid"));

        Assert.IsAssignableFrom<JsonException>(ex.InnerException);
    }

    [Fact]
    public void Iso_date_with_utc_designator_is_kept_as_utc_date_time()
    {
        var filter = new Filter<TestClass>("{\"AnyDate\":{\"_eq\":\"2009-06-15T13:45:30Z\"}}");

        var value = ((Filter<DateTime>)filter.Properties["AnyDate"]).Properties["_eq"];

        var dateTime = Assert.IsType<DateTime>(value);
        Assert.Equal(new DateTime(2009, 6, 15, 13, 45, 30, DateTimeKind.Utc), dateTime);
        Assert.Equal(DateTimeKind.Utc, dateTime.Kind);
    }

    [Fact]
    public void Iso_date_without_time_zone_is_kept_as_written()
    {
        var filter = new Filter<TestClass>("{\"AnyDate\":{\"_gt\":\"2009-06-15\"}}");

        var value = ((Filter<DateTime>)filter.Properties["AnyDate"]).Properties["_gt"];

        Assert.Equal(new DateTime(2009, 6, 15), value);
    }

    [Fact]
    public void Between_dates_are_converted_to_date_time_tuple()
    {
        var filter = new Filter<TestClass>(
            "{\"AnyDate\":{\"_between\":[\"2009-06-15T13:45:30\",\"2010-01-02T00:00:00\"]}}");

        var value = ((Filter<DateTime>)filter.Properties["AnyDate"]).Properties["_between"];

        var tuple = Assert.IsType<ValueTuple<DateTime, DateTime>>(value);
        Assert.Equal(new DateTime(2009, 6, 15, 13, 45, 30), tuple.Item1);
        Assert.Equal(new DateTime(2010, 1, 2), tuple.Item2);
    }

    [Fact]
    public void Between_utc_dates_keep_the_wall_clock_value()
    {
        var filter = new Filter<TestClass>(
            "{\"AnyDate\":{\"_between\":[\"2009-06-15T13:45:30Z\",\"2010-01-02T00:00:00Z\"]}}");

        var value = ((Filter<DateTime>)filter.Properties["AnyDate"]).Properties["_between"];

        var tuple = Assert.IsType<ValueTuple<DateTime, DateTime>>(value);
        Assert.Equal(new DateTime(2009, 6, 15, 13, 45, 30).Ticks, tuple.Item1.Ticks);
        Assert.Equal(new DateTime(2010, 1, 2).Ticks, tuple.Item2.Ticks);
    }

    [Fact]
    public void Between_numbers_are_converted_to_tuple()
    {
        var filter = new Filter<TestClass>("{\"NumberIntergerProp\":{\"_between\":[1,10]}}");

        var value = ((Filter<int>)filter.Properties["NumberIntergerProp"]).Properties["_between"];

        Assert.Equal((1, 10), Assert.IsType<ValueTuple<int, int>>(value));
    }

    [Fact]
    public void Between_with_other_than_two_values_throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Filter<TestClass>("{\"NumberIntergerProp\":{\"_between\":[1,2,3]}}"));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Boolean_values_are_converted(string json, bool expected)
    {
        var filter = new Filter<TestClass>("{\"BooleanProp\":{\"_eq\":" + json + "}}");

        var value = ((Filter<bool>)filter.Properties["BooleanProp"]).Properties["_eq"];

        Assert.Equal(expected, Assert.IsType<bool>(value));
    }

    [Fact]
    public void Numbers_are_converted_to_the_property_type()
    {
        var filter = new Filter<TestClass>(
            "{\"NumberLongProp\":{\"_eq\":9007199254740993},\"NumberDoubleProp\":{\"_eq\":1.5}," +
            "\"Amount\":{\"_eq\":0.1234567890123456789},\"NumberIntergerProp\":{\"_eq\":42}}");

        Assert.Equal(9007199254740993L, ((Filter)filter.Properties["NumberLongProp"]).Properties["_eq"]);
        Assert.Equal(1.5d, ((Filter)filter.Properties["NumberDoubleProp"]).Properties["_eq"]);
        Assert.Equal(0.1234567890123456789m, ((Filter)filter.Properties["Amount"]).Properties["_eq"]);
        Assert.Equal(42, ((Filter)filter.Properties["NumberIntergerProp"]).Properties["_eq"]);
    }

    [Fact]
    public void Numbers_given_as_strings_are_accepted()
    {
        var filter = new Filter<TestClass>(
            "{\"NumberIntergerProp\":{\"_in\":[\"1\",2]},\"NumberLongProp\":{\"_eq\":\"5\"}}");

        Assert.Equal(new[] { 1, 2 }, ((Filter)filter.Properties["NumberIntergerProp"]).Properties["_in"]);
        Assert.Equal(5L, ((Filter)filter.Properties["NumberLongProp"]).Properties["_eq"]);
    }

    [Fact]
    public void Guid_values_are_converted()
    {
        var guid = Guid.NewGuid();
        var filter = new Filter<TestClass>("{\"Uuid\":{\"_eq\":\"" + guid + "\"},\"UuidNullable\":{\"_in\":[\"" + guid + "\"]}}");

        Assert.Equal(guid, ((Filter)filter.Properties["Uuid"]).Properties["_eq"]);
        Assert.Equal(new Guid?[] { guid }, ((Filter)filter.Properties["UuidNullable"]).Properties["_in"]);
    }

    [Fact]
    public void Null_operand_values_are_supported()
    {
        var filter = new Filter<TestClass>("{\"StrProp\":{\"_null\":true},\"AnotherStrProp\":{\"_nnull\":true}}");

        Assert.Equal(2, filter.Properties.Count);
        Assert.IsType<Filter<string>>(filter.Properties["StrProp"]);
    }

    [Fact]
    public void Null_in_array_is_supported_for_nullable_types()
    {
        var filter = new Filter<TestClass>("{\"IntNullable\":{\"_in\":[1,null]}}");

        Assert.Equal(new int?[] { 1, null }, ((Filter)filter.Properties["IntNullable"]).Properties["_in"]);
    }

    [Fact]
    public void Numbers_in_array_are_read_as_strings_for_string_properties()
    {
        // Newtonsoft.Json converted numbers to strings here; keep accepting it.
        var filter = new Filter<TestClass>("{\"StrProp\":{\"_in\":[1,\"b\",2.5]}}");

        Assert.Equal(new[] { "1", "b", "2.5" }, ((Filter)filter.Properties["StrProp"]).Properties["_in"]);
    }

    [Fact]
    public void Empty_object_creates_empty_filter()
    {
        Assert.Empty(new Filter<TestClass>("{}").Properties);
        Assert.Empty(new Filter<TestClass>().Properties);
        Assert.Empty(((Filter)new Filter<TestClass>("{\"Nested\":{}}").Properties["Nested"]).Properties);
    }

    [Fact]
    public void Enums_are_read_by_name_and_by_number()
    {
        var filter = new Filter<EnumHolder>(
            "{\"Color\":{\"_in\":[\"Red\",2]}}");

        Assert.Equal(new[] { Color.Red, Color.Green }, ((Filter)filter.Properties["Color"]).Properties["_in"]);
    }

    [Fact]
    public void Enum_values_are_converted_for_eq()
    {
        var byName = new Filter<EnumHolder>("{\"Color\":{\"_eq\":\"Green\"}}");
        var byNumber = new Filter<EnumHolder>("{\"Color\":{\"_eq\":2}}");

        Assert.Equal(Color.Green, ((Filter)byName.Properties["Color"]).Properties["_eq"]);
        Assert.Equal(Color.Green, ((Filter)byNumber.Properties["Color"]).Properties["_eq"]);
    }

    [Fact]
    public void And_inside_or_is_parsed_and_merged()
    {
        var json = "{\"_or\":[{\"_and\":[{\"StrProp\":{\"_eq\":\"a\"}},{\"NumberIntergerProp\":{\"_gt\":1}}]}," +
                   "{\"StrProp\":{\"_eq\":\"b\"}}]}";

        var filter = new Filter<TestClass>(json);

        var or = Assert.IsType<Filter<TestClass>>(filter.Properties["_or"]);
        Assert.IsType<Filter<TestClass>>(or.Properties["_and"]);
        Assert.IsType<Filter<string>>(or.Properties["StrProp"]);
    }

    [Fact]
    public void Merge_dictionaries_merges_equal_keys()
    {
        var first = FilterDeserializationHelpers.DeserializeJsonRule("{\"n\":{\"_gte\":1}}");
        var second = FilterDeserializationHelpers.DeserializeJsonRule("{\"n\":{\"_lte\":10}}");

        var merged = FilterDeserializationHelpers.MergeDictionaries(new[] { first, second });

        var nested = Assert.IsType<Dictionary<string, object>>(merged["n"]);
        Assert.Equal(new[] { "_gte", "_lte" }, nested.Keys.ToArray());
    }

    [Fact]
    public void Merge_dictionaries_throws_json_exception_for_values_that_are_not_objects()
    {
        var first = FilterDeserializationHelpers.DeserializeJsonRule("{\"n\":1}");
        var second = FilterDeserializationHelpers.DeserializeJsonRule("{\"n\":2}");

        Assert.ThrowsAny<JsonException>(() => FilterDeserializationHelpers.MergeDictionaries(new[] { first, second }));
    }

    [Fact]
    public void Invalid_logical_array_throws_json_exception()
    {
        Assert.ThrowsAny<JsonException>(() => new Filter<TestClass>("{\"_and\":[1,2]}"));
    }

    [Fact]
    public void String_value_that_looks_like_a_date_is_kept_for_string_properties()
    {
        var filter = new Filter<TestClass>("{\"StrProp\":{\"_eq\":\"2009-06-15T13:45:30\"}}");

        Assert.Equal("2009-06-15T13:45:30", ((Filter)filter.Properties["StrProp"]).Properties["_eq"]);
    }
}
