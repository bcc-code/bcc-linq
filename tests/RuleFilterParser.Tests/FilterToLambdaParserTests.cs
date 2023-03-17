using System.Collections.Generic;
using System.Linq;
using RuleFilterParser;
using RuleToLinqParser.Tests.Helpers;

namespace RuleToLinqParser.Tests;

public class FilterToLambdaParserTests
{
    public static readonly List<Person> PeopleList = new()
    {
        new Person("test 1", 15, "Poland", DateTime.Now.AddMonths(-200))
        {
            Car = new Car("Opel", "Vectra", 2005)
        },
        new Person("test 2", 25, "Norway", DateTime.Now.AddMonths(-2))
        {
            Car = new Car("Audi", "A3", 2003)
        },
        new Person("test 3", 20, "Greece", DateTime.Now.AddMonths(-55)),
        new Person("test 4", 23, "Polsomething", DateTime.Now.AddMonths(-67)),
        new Person("test 5", 23, "Poland", DateTime.Now.AddMonths(200)),
        new Person("test 6", 15, "Poland", DateTime.Now.AddMonths(44)),
        new Person("test 7", 23, "Norway", DateTime.Now.AddMonths(10)),
        new Person("test 8", 3, "Sweden", DateTime.Now),
        new Person("test 9", 55, "Sweden", DateTime.Now.AddMonths(60)),
        new Person("test 10", 23, "Poland", DateTime.Now.AddMonths(3)),
    };

    [Fact]
    public void should_get_correct_result_when_filter_has_one_logical_filter()
    {
        var jsonRule =
            "{\"_or\":\r\n[\r\n{\"age\":{\"_gte\": 20}},{\"country\":{\"_eq\":\"Poland\", \"_in\":[\"Poland\", \"Norway\"]}}\r\n]\r\n}";

        var expected = PeopleList.Where(
            person => person.Age >= 20 || person.Country == "Poland" || person.Country.Contains("Pol")).ToList();

        var f = new Filter(jsonRule);
        var exp = FilterToLambdaParser.Parse<Person>(f);
        var result = PeopleList.Where(exp.Compile()).ToList();

        Assert.Equal(expected.Count, result.Count);
    }

    [Fact]
    public void should_get_correct_result_when_filter_has_two_logical_filter()
    {
        var jsonRule = "{\r\n" +
                       "  \"_or\": [\r\n" +
                       "    {\r\n" +
                       "      \"age\": {\r\n" +
                       "        \"_gte\": 20" +
                       "\r\n" +
                       "      }\r\n" +
                       "    },\r\n" +
                       "    {\r\n" +
                       "      \"country\": {" +
                       "\r\n" +
                       "        \"_eq\": \"P" +
                       "oland\",\r\n" +
                       "        \"_in\": [\r" +
                       "\n" +
                       "          \"Greece\"" +
                       ",\r\n" +
                       "          \"Norway\"" +
                       "\r\n" +
                       "        ]\r\n" +
                       "      }\r\n" +
                       "    }\r\n" +
                       "  ],\r\n" +
                       "    \"_and\": [\r\n" +
                       "    {\r\n" +
                       "      \"age\": {\r\n" +
                       "        \"_between\"" +
                       ": [20, 30]\r\n" +
                       "      }\r\n" +
                       "    },\r\n" +
                       "    {\r\n" +
                       "      \"name\": {\r" +
                       "\n" +
                       "        \"_starts_wi" +
                       "th\": \"test\"\r\n" +
                       "      }\r\n" +
                       "    }\r\n" +
                       "  ]\r\n" +
                       "}";

        var expected = PeopleList.Where(person =>
            // or
            (person.Age >= 20 || person.Country == "Poland" || new[] { "Greece", "Norway" }.Contains(person.Country))
            &&
            // and
            ((person.Age >= 20 && person.Age <= 30) && person.Name.StartsWith("test"))).ToList();

        var f = new Filter(jsonRule);
        var exp = FilterToLambdaParser.Parse<Person>(f);
        var result = PeopleList.Where(exp.Compile()).ToList();

        Assert.Equal(expected.Count, result.Count);
    }
    
    [Fact]
    public void should_get_correct_result_when_filtering_nested_object()
    {
        var jsonRule =
            @"{ ""Car"": { ""Model"": { ""_eq"": ""A3"" } } }";

        var expected = PeopleList.Where(person => person.Car.Model == "A3").ToList();
        var f = new Filter(jsonRule);
        var exp = FilterToLambdaParser.Parse<Person>(f);
        var result = PeopleList.Where(exp.Compile()).ToList();

        Assert.Equal(expected.Count, result.Count);
    }
}