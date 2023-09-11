using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BccCode.Linq.Client;

namespace BccCode.Linq.Tests;

public class FetchTests
{
    [Fact(Skip = "FetchAsync doesn't work for Select yet")]
    public async Task FetchAsyncOnSelectTest()
    {
        var api = new ApiClientMockup();

        var names = await api.Persons
                           .Where(p => p.Name != "")
                           .Select(p => p.Name)
                           .FetchAsync();

        Assert.NotNull(names.Data);
    }
}

