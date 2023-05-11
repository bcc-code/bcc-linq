using RuleFilterParser;
using RuleToLinqParser.Tests.Helpers;

namespace RuleToLinqParser.Tests;

public class FilterTests
{
    [Theory]
    [InlineData(
        "{\"_and\":[{\"person\":{\"ability\":{\"_eq\":\"mentor\"}}},{\"person\":{\"age\":{\"_eq\":\"15\"}}}]}")]
    [InlineData(
        "{\"_and\":[{\"_and\":[{\"org\":{\"country\":{\"_eq\":\"AD\"}}},{\"person\":{\"age\":{\"_gt\":\"15\"}}}]}]}")]
    [InlineData(
        "{\"_and\":[{\"_or\":[{\"register\":{\"as\":{\"_eq\":\"mentee\"}}},{\"person\":{\"age\":{\"_lt\":\"22\"}}},{\"person\":{\"age\":{\"_gt\":\"11\"}}}]},{\"_and\":[{\"person\":{\"active\":{\"_eq\":true}}}]}]}")]
    public void should_deserialize_json_with_square_brackets_correctly(string json)
    {
        var exception = Record.Exception(() => new Filter<TestClass>(json));
        Assert.Null(exception);
    }

    [Fact]
    public void should_throw_arg_exception_when_json_is_incorrect()
    {
        var json = @"{""_and"":  [ 
                            ""test"": { ""_in"": [1, 2, 3] }, 
                            ""test2"": { ""_eq"": ""str"" } 
                        
                     ]";

        Assert.Throws<ArgumentException>(() => new Filter<TestClass>(json));
    }

    [Fact]
    public void should_throw_arg_exception_when_json_is_null()
    {
        var json = "";
        Assert.Throws<ArgumentException>(() => new Filter<TestClass>(json));
    }

    [Fact]
    public void should_to_filter_type_under_logical_filter()
    {
        var json = @"{""_and"":  [ 
                            { ""test"": { ""_in"": [1, 2, 3] } }, 
                            { ""test2"": { ""_eq"": ""str"" } } 
                        ]
                     }";
        var filter = new Filter<TestClass>(json);

        var value = filter.Properties["_and"];

        Assert.IsType<Filter<TestClass>>(value);
    }

    [Fact]
    public void should_cast_value_to_double_list()
    {
        var json = @"{ ""NumberDoubleProp"": { ""_in"": [1, 2, 3] } }";
        var filter = new Filter<TestClass>(json);

        var value = ((Filter<double>)filter.Properties["NumberDoubleProp"]).Properties["_in"];

        Assert.IsAssignableFrom<IEnumerable<double>>(value);
    }
    
    [Fact]
    public void should_cast_value_to_decimal_list()
    {
        var json = @"{ ""Amount"": { ""_in"": [100, 200, 300] } }";
        
        var filter = new Filter<TestClass>(json);
        var value = ((Filter<decimal>)filter.Properties["Amount"]).Properties["_in"];

        Assert.IsAssignableFrom<IEnumerable<decimal>>(value);
    }

    [Fact]
    public void should_throw_exception_on_empty_list()
    {
        var json = @"{ ""StringArrayProp"": { ""_in"": [] } }";

        Assert.Throws<ArgumentException>(() => new Filter<TestClass>(json));
    }

    [Fact]
    public void should_cast_value_to_double_tuple()
    {
        var json = @"{ ""NumberDoubleProp"": { ""_between"": [1, 2] } }";
        var filter = new Filter<TestClass>(json);

        var value = ((Filter<double>)filter.Properties["NumberDoubleProp"]).Properties["_between"];
        Assert.IsType<(double, double)>(value);
    }
    
    [Fact]
    public void should_cast_value_to_datetime_tuple()
    {
        var json = @"{ ""AnyDate"": { ""_between"": [""2023-01-01T00:00:00.0000000"", ""2023-03-01T00:00:00.0000000""] } }";
        var filter = new Filter<TestClass>(json);

        var value = ((Filter<DateTime>)filter.Properties["AnyDate"]).Properties["_between"];
        Assert.IsType<(DateTime, DateTime)>(value);
    }

    [Fact]
    public void should_throw_exception_on_not_pair_value()
    {
        var json = @"{ ""NumberDoubleProp"": { ""_between"": [1, 2, 3, 4] } }";

        Assert.Throws<ArgumentException>(() => new Filter<TestClass>(json));
    }

    [Theory]
    [InlineData(20)]
    [InlineData("20")]
    public void should_cast_value_to_double(object val)
    {
        var json = $@"{{ ""NumberDoubleProp"": {{ ""_eq"": {val} }} }}";
        var filter = new Filter<TestClass>(json);

        var value = ((Filter<double>)filter.Properties["NumberDoubleProp"]).Properties["_eq"];

        Assert.IsType<double>(value);
    }


    [Fact]
    public void should_cast_value_to_string()
    {
        var json = @"{ ""StrProp"": { ""_eq"": ""str"" } }";
        var filter = new Filter<TestClass>(json);

        var value = ((Filter<string>)filter.Properties["StrProp"]).Properties["_eq"];

        Assert.IsType<string>(value);
    }

    [Fact]
    public void should_cast_value_to_bool()
    {
        var json = @"{ ""BooleanProp"": { ""_eq"": true } }";
        var filter = new Filter<TestClass>(json);

        var value = ((Filter<bool>)filter.Properties["BooleanProp"]).Properties["_eq"];

        Assert.IsType<bool>(value);
    }

    [Fact]
    public void should_cast_value_to_date()
    {
        var json = @"{ ""AnyDate"": { ""_eq"": ""2009-06-15T13:45:30"" } }";
        var filter = new Filter<TestClass>(json);

        var value = ((Filter<DateTime>)filter.Properties["AnyDate"]).Properties["_eq"];

        Assert.IsType<DateTime>(value);
    }

    [Fact]
    public void should_not_cast_value_to_date()
    {
        var json = @"{ ""AnyDate"": { ""_eq"": ""2009-gd06-15T13:45:30"" } }";
        var filter = new Filter<TestClass>(json);

        var value = ((Filter<DateTime>)filter.Properties["AnyDate"]).Properties["_eq"];

        Assert.IsNotType<DateTime>(value);
    }
}
