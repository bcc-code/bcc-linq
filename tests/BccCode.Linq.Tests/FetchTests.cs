using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BccCode.Linq.Tests;

public class FetchTests
{
    [Fact]
    public async Task FetchAsyncOnSelectTest()
    {
        var api = new ApiClientMockup();

        var names = await api.Persons
                           .Where(p => p.Name != "")
                           .Select(p => p.Name)
                           .FetchAsync();

        Assert.NotNull(names);
    }
}

