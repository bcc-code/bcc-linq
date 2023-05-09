using RuleFilterParser;

namespace RuleToLinqParser.Tests;

public class SerializeFilterTest
{
    [Fact]
    public void SerializeFilter()
    {
        var jsonString =  @"{""_and"":  [ 
                            { ""test"": { ""_in"": [1, 2, 3] } }, 
                            { ""test2"": { ""_eq"": ""str"" } } 
                        ]
                     }";
        var filter = new Filter(jsonString);

        var filterString = filter.ToString();
        
        Assert.Equal(jsonString, filterString);

    }
}