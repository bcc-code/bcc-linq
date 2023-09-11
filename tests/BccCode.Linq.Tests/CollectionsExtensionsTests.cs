using BccCode.Linq.Extensions;
using BccCode.Linq.Server;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests;

public class CollectionsExtensionsTests
{
    [Fact]
    public void should_get_same_result_as_using_parser_directly()
    {
        var jsonRule =
            "{\"_or\":\r\n[\r\n{\"age\":{\"_gte\": 20}},{\"country\":{\"_eq\":\"Poland\", \"_in\":[\"Poland\", \"Norway\"]}}\r\n]\r\n}";

        var f = new Filter<Person>(jsonRule);
        var expression = FilterToLambdaParser.Parse(f);
        var result = FilterToLambdaParserTests.PeopleList.Where(expression.Compile()).ToList();

        var resultWithExtensionMethod = FilterToLambdaParserTests.PeopleList.ApplyRuleFilter(f).ToList();

        Assert.Equal(result.Count, resultWithExtensionMethod.Count);
    }
}
