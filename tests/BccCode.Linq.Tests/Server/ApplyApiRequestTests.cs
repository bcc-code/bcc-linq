using System.ComponentModel;
using System.Reflection;
using BccCode.Linq.Server;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests.Server;

public class ApplyApiRequestTests
{
    [Fact]
    public void TestGetSortingFromRequest()
    {
        var structuredSorting = CollectionsExtensions.GetSorting<TestClass>($"-{nameof(TestClass.AnyDate)},{nameof(TestClass.Uuid)}").ToList();
        var expectedStructSorting = new List<(PropertyInfo?, ListSortDirection Descending)>
        {
            (typeof(TestClass).GetProperty(nameof(TestClass.AnyDate)), ListSortDirection.Descending),
            (typeof(TestClass).GetProperty(nameof(TestClass.Uuid)), ListSortDirection.Ascending),
        };

        Assert.NotNull(structuredSorting);
        Assert.NotNull(expectedStructSorting);
        Assert.Equal(expectedStructSorting, structuredSorting);
    }
}
