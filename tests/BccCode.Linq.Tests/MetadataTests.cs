using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BccCode.Linq.ApiClient;
using BccCode.Linq.ApiClient.Immutable;
using BccCode.Linq.Tests.Helpers;

namespace RuleToLinqParser.Tests;

public class MetadataTests
{
    [Fact]
    public void TotalCountFromTotal()
    {
        var resultList = new ResultList<Person>
        {
            Data = new List<Person>(),
            Meta = new Metadata(new Dictionary<string, object>
            {
                { "total", 4 }
            })
        }.ToImmutableResultList();
        
        Assert.Equal(4, resultList.Meta.TotalCount);


    }
}

