using System.ComponentModel;
using System.Reflection;
using BccCode.Linq.Server;
using BccCode.Linq.Tests.Helpers;
using BccCode.Platform;

namespace BccCode.Linq.Tests.Server;

/// <summary>
/// A test class holding tests for the <see cref="CollectionsExtensions.ApplyApiRequest"/> static method.
/// </summary>
public class ApplyApiRequestTests
{
    /// <summary>
    /// Tests if the <see cref="CollectionsExtensions.GetSorting"/> method parses the
    /// sorting string correctly.
    /// </summary>
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

    /// <summary>
    /// Tests if the default sorting is used.
    /// </summary>
    [Fact]
    public void TestUseDefaultSorting()
    {
        var personsQueryable = Seeds.Persons.AsQueryable();

        var result = personsQueryable
            .ApplyApiRequest(new QueryableParameters(), defaultSort: $"{nameof(Person.Age)}")
            .ToList();
        var expectedResult = personsQueryable
            .OrderBy(p => p.Age)
            .ToList();

        Assert.Equal(expectedResult, result);
    }

    /// <summary>
    /// Tests if a custom sorting applied via the <see cref="QueryableParameters"/>
    /// overwrites the default sorting passed to the <see cref="CollectionsExtensions.ApplyApiRequest"/> method.
    /// </summary>
    [Fact]
    public void TestUseCustomSortingOverDefaultSorting()
    {
        var personsQueryable = Seeds.Persons.AsQueryable();

        var result = personsQueryable
            .ApplyApiRequest(new QueryableParameters
                {
                    Sort = $"-{nameof(Person.Age)}"
                }, defaultSort: $"{nameof(Person.Age)}")
            .ToList();
        var expectedResult = personsQueryable
            .OrderByDescending(p => p.Age)
            .ToList();

        Assert.Equal(expectedResult, result);
    }
}
