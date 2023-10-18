using System.Text;
using BccCode.Linq.Client;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests;

public class LinqQueryProviderTests
{
    #region Any

    [Fact]
    public void AnyTest()
    {
        var api = new ApiClientMockup();

        var anyResult = api.Persons.Any();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.True(anyResult);
    }

    [Fact]
    public void AnyPredicateTest()
    {
        var api = new ApiClientMockup();

        var anyResult = api.Persons.Any(p => p.Age > 26);
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("{\"age\": {\"_gt\": 26}}", api.ClientQuery?.Filter);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.True(anyResult);
    }

    [Fact]
    public async void AnyAsyncTest()
    {
        var api = new ApiClientMockup();

        var anyResult = await api.Persons.AnyAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.True(anyResult);
    }

    [Fact]
    public async void AnyPredicateAsyncTest()
    {
        var api = new ApiClientMockup();

        var anyResult = await api.Persons.AnyAsync(p => p.Age > 26);
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("{\"age\": {\"_gt\": 26}}", api.ClientQuery?.Filter);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.True(anyResult);
    }

    #endregion

    #region ElementAt

    [Fact]
    public void ElementAtTest()
    {
        var api = new ApiClientMockup();

        var persons = api.Persons.ElementAt(2);
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Equal(2, api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret ElementAt clauses. Since we remove the ElementAt clause
        //       from the expression tree, the result will be still the first element of the mockup data.
        //Assert.Equal("Chelsey Logan", persons.Name);
        Assert.Equal("Archibald Mcbride", persons.Name);
    }

    [Fact]
    public async void ElementAtAsyncTest()
    {
        var api = new ApiClientMockup();

        var persons = await api.Persons.ElementAtAsync(2);
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Equal(2, api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret ElementAt clauses. Since we remove the ElementAt clause
        //       from the expression tree, the result will be still the first element of the mockup data.
        //Assert.Equal("Chelsey Logan", persons.Name);
        Assert.Equal("Archibald Mcbride", persons.Name);
    }

    #endregion

    #region ElementAtOrDefault

    [Fact]
    public void ElementAtOrDefaultTest()
    {
        var api = new ApiClientMockup();

        var persons = api.Persons.ElementAtOrDefault(2);
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Equal(2, api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret ElementAt clauses. Since we remove the ElementAt clause
        //       from the expression tree, the result will be still the first element of the mockup data.
        //Assert.Equal("Chelsey Logan", persons.Name);
        Assert.Equal("Archibald Mcbride", persons?.Name);
    }

    [Fact]
    public async void ElementAtOrDefaultAsyncTest()
    {
        var api = new ApiClientMockup();

        var persons = await api.Persons.ElementAtOrDefaultAsync(2);
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Equal(2, api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret ElementAt clauses. Since we remove the ElementAt clause
        //       from the expression tree, the result will be still the first element of the mockup data.
        //Assert.Equal("Chelsey Logan", persons.Name);
        Assert.Equal("Archibald Mcbride", persons?.Name);
    }

    [Fact]
    public void ElementAtOrDefaultNotFoundTest()
    {
        var api = new ApiClientMockup();

        var persons = api.Persons.ElementAtOrDefault(int.MaxValue);
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Equal(int.MaxValue, api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret ElementAt clauses. Since we remove the ElementAt clause
        //       from the expression tree, the result will be still the first element of the mockup data.
        //Assert.Equal(null, persons.Name);
        Assert.Equal("Archibald Mcbride", persons?.Name);
    }

    [Fact]
    public void ElementAtOrDefaultEmptyTest()
    {
        var api = new ApiClientMockup();

        var testClass = api.Empty.ElementAtOrDefault(0);
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.Null(testClass);
    }

    [Fact]
    public async void ElementAtOrDefaultNotFoundAsyncTest()
    {
        var api = new ApiClientMockup();

        var persons = await api.Persons.ElementAtOrDefaultAsync(int.MaxValue);
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Equal(int.MaxValue, api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret ElementAt clauses. Since we remove the ElementAt clause
        //       from the expression tree, the result will be still the first element of the mockup data.
        //Assert.Equal(null, persons.Name);
        Assert.Equal("Archibald Mcbride", persons?.Name);
    }

    #endregion

    #region First

    [Fact]
    public void FirstTest()
    {
        var api = new ApiClientMockup();

        var persons = api.Persons.First();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.Equal("Archibald Mcbride", persons.Name);
    }

    [Fact]
    public async void FirstAsyncTest()
    {
        var api = new ApiClientMockup();

        var persons = await api.Persons.FirstAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.Equal("Archibald Mcbride", persons.Name);
    }

    #endregion

    #region FirstOrDefault

    [Fact]
    public void FirstOrDefaultTest()
    {
        var api = new ApiClientMockup();

        var persons = api.Persons.FirstOrDefault();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.Equal("Archibald Mcbride", persons?.Name);
    }

    [Fact]
    public void FirstOrDefaultEmptyTest()
    {
        var api = new ApiClientMockup();

        var testClass = api.Empty.FirstOrDefault();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.Null(testClass);
    }

    [Fact]
    public async void FirstOrDefaultAsyncTest()
    {
        var api = new ApiClientMockup();

        var persons = await api.Persons.FirstOrDefaultAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.Equal("Archibald Mcbride", persons?.Name);
    }

    [Fact]
    public async void FirstOrDefaultEmptyAsyncTest()
    {
        var api = new ApiClientMockup();

        var testClass = await api.Empty.FirstOrDefaultAsync();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.Null(testClass);
    }

    #endregion

    #region Search

    [Fact]
    public void SearchConstantTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons.Search("Chuck Norris");

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal("Chuck Norris", api.ClientQuery?.Search);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SearchStringBuilderTest()
    {
        var api = new ApiClientMockup();

        var sb = new StringBuilder();
        sb.Append("Chuck");
        sb.Append(' ');
        sb.Append("Norris");

        var query = api.Persons.Search(sb.ToString());

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal("Chuck Norris", api.ClientQuery?.Search);
        Assert.Equal(5, persons.Count);
    }

    #endregion

    #region Select

    [Fact]
    public void SelectTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectSingleStringFieldTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select p.Name;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("name", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectSingleIntegerFieldTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select p.Age;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("age", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectNewSingleFieldTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select new
            {
                p.Name
            };

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("name", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectNewNestedSinglePropertyTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons
            .OrderByDescending(p => p.Name)
            .Select(a => new
            {
                r = a.Car
            });

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("car.*", api.ClientQuery?.Fields);
        Assert.Equal("-name", api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectNewNestedSingleFieldIfNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select new
            {
                Manufacturer = p.Car == null ? null : p.Car.Manufacturer,
                ManufacturerInfo = p.Car == null ? null : p.Car.ManufacturerInfo
            };

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("car.*,car.manufacturer,car.manufacturerInfo.*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectNewNestedFlattendResultTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select new
            {
                Year = p.Car == null ? 0 : p.Car.YearOfProduction,
                ManufacturerName = p.Car.ManufacturerInfo.Name
            };

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("car.*,car.yearOfProduction,car.manufacturerInfo.name", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectNewTwoFieldsTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select new
            {
                p.Name,
                p.Age
            };

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("name,age", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    #endregion

    #region Single

    [Fact]
    public void SingleTest()
    {
        var api = new ApiClientMockup();

        Assert.Throws<InvalidOperationException>(() =>
        {
            // ReSharper disable once UnusedVariable
            var persons = api.Persons.Single();
        });
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(2, api.ClientQuery?.Limit);
    }

    [Fact]
    public void SingleAsyncTest()
    {
        var api = new ApiClientMockup();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            // ReSharper disable once UnusedVariable
            var persons = await api.Persons.SingleAsync();
        });
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(2, api.ClientQuery?.Limit);
    }

    #endregion

    #region SingleOrDefault

    [Fact]
    public void SingleOrDefaultTest()
    {
        var api = new ApiClientMockup();

        Assert.Throws<InvalidOperationException>(() =>
        {
            // ReSharper disable once UnusedVariable
            var persons = api.Persons.SingleOrDefault();
        });
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(2, api.ClientQuery?.Limit);
    }

    [Fact]
    public void SingleOrDefaultEmptyTest()
    {
        var api = new ApiClientMockup();

        var testClass = api.Empty.SingleOrDefault();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(2, api.ClientQuery?.Limit);
        Assert.Null(testClass);
    }

    [Fact]
    public void SingleOrDefaultAsyncTest()
    {
        var api = new ApiClientMockup();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            // ReSharper disable once UnusedVariable
            var persons = await api.Persons.SingleOrDefaultAsync();
        });
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(2, api.ClientQuery?.Limit);
    }

    [Fact]
    public async void SingleOrDefaultEmptyAsyncTest()
    {
        var api = new ApiClientMockup();

        var testClass = await api.Empty.SingleOrDefaultAsync();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(2, api.ClientQuery?.Limit);
        Assert.Null(testClass);
    }

    #endregion

    #region Enums

    [Fact]
    public void NumericEnumTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons.Where(t => t.Type == PersonType.Staff);

        var persons = query.ToList();

        Assert.Equal("{\"type\": {\"_eq\": 2}}", api.ClientQuery?.Filter);

    }

    [Fact]
    public void StringEnumTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons.Where(t => t.Type.ToString() == PersonType.Staff.ToString());

        var persons = query.ToList();

        Assert.Equal("{\"type\": {\"_eq\": \"Staff\"}}", api.ClientQuery?.Filter);

    }


    [Fact(Skip = "This approach does not work.")]
    public void StringEnumAltTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons.Where(t => t.Type.ToString().Equals(PersonType.Staff));

        var persons = query.ToList();

        Assert.Equal("{\"type\": {\"_eq\": \"Staff\"}}", api.ClientQuery?.Filter);

    }

    #endregion

    #region Where

    #region Where property equal to ...

    [Fact]
    public void WhereGuidEqualStaticNewTest()
    {
        var api = new ApiClientMockup();

        var query =
            from m in api.Manufacturers
                // here the constructor for 'Guid' is called during expression tree validation (client side invocation)
            where m.Uid == new Guid("16b41e40-ee3c-4837-b9a9-57b76c7d1d9d")
            select m;

        var manufacturer = query.FirstOrDefault();
        Assert.Equal("manufacturers", api.PageEndpoint);
        Assert.Equal("{\"uid\": {\"_eq\": \"16b41e40-ee3c-4837-b9a9-57b76c7d1d9d\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.NotNull(manufacturer);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the first row of the mockup data.
        //Assert.Equal(new Guid("16b41e40-ee3c-4837-b9a9-57b76c7d1d9d"), manufacturer?.Uid);
        Assert.Equal(new Guid("4477e983-be5c-43d5-b3d0-5f26971bf2f3"), manufacturer?.Uid);
    }

    [Fact]
    public void WhereGuidEqualStaticMethodCallTest()
    {
        var api = new ApiClientMockup();

        var query =
            from m in api.Manufacturers
                // here the method 'Guid.Parse' is called during expression tree validation (client side invocation)
            where m.Uid == Guid.Parse("16b41e40-ee3c-4837-b9a9-57b76c7d1d9d")
            select m;

        var manufacturer = query.FirstOrDefault();
        Assert.Equal("manufacturers", api.PageEndpoint);
        Assert.Equal("{\"uid\": {\"_eq\": \"16b41e40-ee3c-4837-b9a9-57b76c7d1d9d\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.NotNull(manufacturer);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the first row of the mockup data.
        //Assert.Equal(new Guid("16b41e40-ee3c-4837-b9a9-57b76c7d1d9d"), manufacturer?.Uid);
        Assert.Equal(new Guid("4477e983-be5c-43d5-b3d0-5f26971bf2f3"), manufacturer?.Uid);
    }

    [Fact]
    public void WhereGuidEqualLocalVariableTest()
    {
        var api = new ApiClientMockup();

        var uid = Guid.Parse("16b41e40-ee3c-4837-b9a9-57b76c7d1d9d");

        var query =
            from m in api.Manufacturers
                // here we use a local variable which will be evaluated on client side
            where m.Uid == uid
            select m;

        var manufacturer = query.FirstOrDefault();
        Assert.Equal("manufacturers", api.PageEndpoint);
        Assert.Equal("{\"uid\": {\"_eq\": \"16b41e40-ee3c-4837-b9a9-57b76c7d1d9d\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.NotNull(manufacturer);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the first row of the mockup data.
        //Assert.Equal(new Guid("16b41e40-ee3c-4837-b9a9-57b76c7d1d9d"), manufacturer?.Uid);
        Assert.Equal(new Guid("4477e983-be5c-43d5-b3d0-5f26971bf2f3"), manufacturer?.Uid);
    }

    [Fact]
    public void WhereGuidEqualRemoteToStringTest()
    {
        var api = new ApiClientMockup();

        var query =
            from m in api.Manufacturers

                // It is invalid to call a method on a server-side property. This will not work.
            where m.Uid.ToString() == "16b41e40-ee3c-4837-b9a9-57b76c7d1d9d"
            select m;

        var manufacturer = query.FirstOrDefault();
        Assert.Equal("manufacturers", api.PageEndpoint);
        Assert.Equal("{\"uid\": {\"_eq\": \"16b41e40-ee3c-4837-b9a9-57b76c7d1d9d\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(1, api.ClientQuery?.Limit);
        Assert.NotNull(manufacturer);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the first row of the mockup data.
        //Assert.Equal(new Guid("16b41e40-ee3c-4837-b9a9-57b76c7d1d9d"), manufacturer?.Uid);
        Assert.Equal(new Guid("4477e983-be5c-43d5-b3d0-5f26971bf2f3"), manufacturer?.Uid);
    }

    [Fact]
    public void WhereStringEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.StrProp == null
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"strProp\": {\"_eq\": null}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereStringEqualToEmptyStringTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.StrProp == ""
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"strProp\": {\"_eq\": \"\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereIntegerEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.NumberIntergerProp == 5
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"numberIntergerProp\": {\"_eq\": 5}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereIntegerNullableEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.IntNullable == null
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"intNullable\": {\"_eq\": null}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereIntegerNullableEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.IntNullable == 5
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"intNullable\": {\"_eq\": 5}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDoubleEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
                // ReSharper disable once CompareOfFloatsByEqualityOperator
            where p.NumberDoubleProp == -5.13
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"numberDoubleProp\": {\"_eq\": -5.13}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereLongEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.NumberLongProp == 3372036854775807
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"numberLongProp\": {\"_eq\": 3372036854775807}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereBoolEqualToTrueTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.BooleanProp == true
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"booleanProp\": {\"_eq\": true}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDecimalEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.Amount == 312312.5434353m
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"amount\": {\"_eq\": 312312.5434353}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDecimalNullableEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.AmountNullable == null
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"amountNullable\": {\"_eq\": null}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDecimalNullableEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.AmountNullable == 312312.5434353m
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"amountNullable\": {\"_eq\": 312312.5434353}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDateTimeEqualToDateTimeTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.AnyDate == new DateTime(2013, 12, 4, 4, 2, 5, DateTimeKind.Utc)
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"anyDate\": {\"_eq\": \"2013-12-04T04:02:05.0000000Z\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDateTimeNullableEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.DateNullable == null
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"dateNullable\": {\"_eq\": null}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDateTimeNullableEqualToDateTimeTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.DateNullable == new DateTime(2013, 12, 4, 4, 2, 5, DateTimeKind.Utc)
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"dateNullable\": {\"_eq\": \"2013-12-04T04:02:05.0000000Z\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void WhereDateOnlyNullableEqualToDateTimeTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.DateOnly == new DateOnly(2013, 12, 4)
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"dateOnly\": {\"_eq\": \"2013-12-04\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }
#endif

    [Fact]
    public void WhereGuidEqualToGuidTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.Uuid == new Guid("00000000-0000-0000-0000-000000000000")
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"uuid\": {\"_eq\": \"00000000-0000-0000-0000-000000000000\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereGuidNullableEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.UuidNullable == null
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"uuidNullable\": {\"_eq\": null}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereGuidNullableEqualToGuidTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.UuidNullable == new Guid("00000000-0000-0000-0000-000000000000")
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"uuidNullable\": {\"_eq\": \"00000000-0000-0000-0000-000000000000\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    #endregion

    #region Where property.ToString() to ...

    [Fact]
    public void WhereStringToStringEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.StrProp.ToString() == null
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"strProp\": {\"_eq\": null}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereStringToStringEqualToEmptyStringTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.StrProp.ToString() == ""
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"strProp\": {\"_eq\": \"\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereIntegerToStringEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.NumberIntergerProp.ToString() == "5"
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereIntegerNullableToStringEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.IntNullable.ToString() == null
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereIntegerNullableToStringEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.IntNullable.ToString() == "5"
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereDoubleToStringEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            where p.NumberDoubleProp.ToString() == "-5.13"
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereLongToStringEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.NumberLongProp.ToString() == "3372036854775807"
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereBoolToStringEqualToTrueTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.BooleanProp.ToString() == "True"
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereDecimalToStringEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            where p.Amount.ToString() == "312312.5434353"
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereDecimalNullableToStringEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.AmountNullable.ToString() == null
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereDecimalNullableToStringEqualToNumberTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.AmountNullable.ToString() == "312312.5434353"
            select p;

        Assert.Throws<NotSupportedException>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void WhereDateTimeToStringEqualToDateTimeTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            where p.AnyDate.ToString() == "2023-12-04T04:02:05Z"
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"anyDate\": {\"_eq\": \"2023-12-04T04:02:05Z\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDateTimeNullableToStringEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.DateNullable.ToString() == null
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"dateNullable\": {\"_eq\": null}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereDateTimeNullableToStringEqualToDateTimeTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.DateNullable.ToString() == "2023-12-04T04:02:05Z"
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"dateNullable\": {\"_eq\": \"2023-12-04T04:02:05Z\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void WhereDateOnlyToStringEqualToDateTimeTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.DateOnly.ToString() == "2023-12-04"
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"dateOnly\": {\"_eq\": \"2023-12-04\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }
#endif

    [Fact]
    public void WhereGuidToStringEqualToGuidTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.Uuid.ToString() == "00000000-0000-0000-0000-000000000000"
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"uuid\": {\"_eq\": \"00000000-0000-0000-0000-000000000000\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereGuidNullableToStringEqualToNullTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.UuidNullable.ToString() == null
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"uuidNullable\": {\"_eq\": null}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereGuidNullableToStringEqualToGuidTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where p.UuidNullable.ToString() == "00000000-0000-0000-0000-000000000000"
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"uuidNullable\": {\"_eq\": \"00000000-0000-0000-0000-000000000000\"}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    #endregion

    [Fact]
    public void WhereContainsInlineGuidArrayTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Empty
            where new[] { Guid.Empty }.Contains(p.Uuid)
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"uuid\": {\"_in\": [\"00000000-0000-0000-0000-000000000000\"]}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereContainsVariableGuidArrayTest()
    {
        var api = new ApiClientMockup();

        var uuids = new[] { Guid.Empty };

        var query =
            from p in api.Empty
            where uuids.Contains(p.Uuid)
            select p;

        var persons = query.ToList();
        Assert.Equal("empty", api.PageEndpoint);
        Assert.Equal("{\"uuid\": {\"_in\": [\"00000000-0000-0000-0000-000000000000\"]}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Empty(persons);
    }

    [Fact]
    public void WhereIntGreaterThanTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("{\"age\": {\"_gt\": 26}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void WhereIntGreaterThanAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("{\"age\": {\"_gt\": 26}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereIntNotGreaterThanTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where !(p.Age > 26)
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("{\"age\": {\"_lte\": 26}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void WhereIntNotGreaterThanAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where !(p.Age > 26)
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("{\"age\": {\"_lte\": 26}}", api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereSelectAndTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26 && p.Name == "Reid Cantrell"
            select new
            {
                p.Country
            };

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("{\"_and\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Reid Cantrell\"}}]}",
            api.ClientQuery?.Filter);
        Assert.Equal("country", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereBetweenTest()
    {
        var api = new ApiClientMockup();
        var query = api.Empty.Where(x => x.Nested.Number >= 1 && x.Nested.Number <= 10).ToList();

        Assert.Equal(
            "{\"_and\": [{\"nested\": {\"number\": {\"_gte\": 1}}}, {\"nested\": {\"number\": {\"_lte\": 10}}}]}",
            api.ClientQuery?.Filter);
    }


    [Fact]
    public async void WhereSelectAndAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26 && p.Name == "Reid Cantrell"
            select new
            {
                p.Country
            };

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("{\"_and\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Reid Cantrell\"}}]}",
            api.ClientQuery?.Filter);
        Assert.Equal("country", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);

        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereSelectOrTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26 || p.Name == "Chelsey Logan"
            select new
            {
                p.Country,
                p.Name
            };

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("{\"_or\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Chelsey Logan\"}}]}",
            api.ClientQuery?.Filter);
        Assert.Equal("country,name", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void WhereSelectOrAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26 || p.Name == "Chelsey Logan"
            select new
            {
                p.Country,
                p.Name
            };

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal("{\"_or\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Chelsey Logan\"}}]}",
            api.ClientQuery?.Filter);
        Assert.Equal("country,name", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereSelectOrTwiceTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26 || p.Name == "Chelsey Logan" || p.Country == "US"
            select new
            {
                p.Country,
                p.Name
            };

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"_or\": [{\"_or\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Chelsey Logan\"}}]}, {\"country\": {\"_eq\": \"US\"}}]}",
            api.ClientQuery?.Filter);
        Assert.Equal("country,name", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(4, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void WhereSelectOrTwiceAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26 || p.Name == "Chelsey Logan" || p.Country == "US"
            select new
            {
                p.Country,
                p.Name
            };

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"_or\": [{\"_or\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Chelsey Logan\"}}]}, {\"country\": {\"_eq\": \"US\"}}]}",
            api.ClientQuery?.Filter);
        Assert.Equal("country,name", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(4, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void StringStartsWithTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Name.StartsWith("Chelsey")
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"name\": {\"_starts_with\": \"Chelsey\"}}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void StringStartsWithAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Name.StartsWith("Chelsey")
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"name\": {\"_starts_with\": \"Chelsey\"}}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereStringEndsWithTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Name.EndsWith("Cantrell")
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"name\": {\"_ends_with\": \"Cantrell\"}}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void WhereStringEndsWithAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Name.EndsWith("Cantrell")
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"name\": {\"_ends_with\": \"Cantrell\"}}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereStringIsNullOrEmptyTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where string.IsNullOrEmpty(p.Name)
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"name\": {\"_empty\": null}}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(0, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void WhereStringIsNullOrEmptyAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where string.IsNullOrEmpty(p.Name)
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"name\": {\"_empty\": null}}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(0, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereEqualValueTypeTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where Equals(p.Age, 13)
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"age\": {\"_eq\": 13}}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereNestedEqualTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Car != null && Equals(p.Car.Model, "Opel")
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"_and\": [{\"car\": {\"_neq\": null}}, {\"car\": {\"model\": {\"_eq\": \"Opel\"}}}]}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void WhereNestedEqualAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Car != null && Equals(p.Car.Model, "Opel")
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Equal(
            "{\"_and\": [{\"car\": {\"_neq\": null}}, {\"car\": {\"model\": {\"_eq\": \"Opel\"}}}]}",
            api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    #endregion

    #region OrderBy

    [Fact]
    public void OrderByTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Name
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("name", api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void OrderByAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Name
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("name", api.ClientQuery?.Sort);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void OrderByDescendingTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Name descending
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("-name", api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void OrderByDescendingAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Name descending
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("-name", api.ClientQuery?.Sort);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void OrderByMultipleColumnsTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Name, p.Age descending, p.Country
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("name,-age,country", api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void OrderByMultipleColumnsAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Name, p.Age descending, p.Country
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("name,-age,country", api.ClientQuery?.Sort);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void OrderByNestingTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Car.Manufacturer
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("car.manufacturer", api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        Assert.Equal(5, persons.Count);
    }

    #endregion

    #region Skip

    [Fact]
    public void SkipTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons.Skip(2);

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Equal(2, api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Take clauses. Since we remove the Take clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    #endregion

    #region Take

    [Fact]
    public void TakeTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons.Take(3);

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Equal(3, api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Take clauses. Since we remove the Take clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SkipTakeTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons.Skip(2).Take(3);

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Equal(2, api.ClientQuery?.Offset);
        Assert.Equal(3, api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Take clauses. Since we remove the Take clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public async void OrderByNestingAsyncTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Car.Manufacturer
            select p;

        var persons = await query.ToListAsync();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*", api.ClientQuery?.Fields);
        Assert.Equal("car.manufacturer", api.ClientQuery?.Sort);
        Assert.Equal(5, persons.Count);
    }

    #endregion

    #region Include

    [Fact]
    public void IncludeTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons
            .Include(p => p.Car);

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*,car.*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Take clauses. Since we remove the Take clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void IncludeSecondNestedTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons
            .Include(p => p.Car.Manufacturer);

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*,car.manufacturer.*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Take clauses. Since we remove the Take clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void IncludeThenIncludeTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons
            .Include(p => p.Car)
                .ThenInclude(c => c.ManufacturerInfo);

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*,car.*,car.manufacturerInfo.*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Take clauses. Since we remove the Take clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }


    [Fact]
    public void IncludeThenIncludeAfterEnumerableTest()
    {
        var api = new ApiClientMockup();

        var query = api.Persons
            .Include(p => p.CarHistory).ThenInclude(c => c.ManufacturerInfo)
            ;

        var persons = query.ToList();
        Assert.Equal("persons", api.PageEndpoint);
        Assert.Null(api.ClientQuery?.Filter);
        Assert.Equal("*,carHistory.*,carHistory.manufacturerInfo.*", api.ClientQuery?.Fields);
        Assert.Null(api.ClientQuery?.Sort);
        Assert.Null(api.ClientQuery?.Offset);
        Assert.Null(api.ClientQuery?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Take clauses. Since we remove the Take clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    #endregion


}
