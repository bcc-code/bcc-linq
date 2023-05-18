using RuleFilterParser;
using RuleToLinqParser.Tests.Helpers;
using RuleToLinqParser.Tests.Seeds;

namespace RuleToLinqParser.Tests;

public class LinqQueryProviderTests
{
    private ApiClientMockup InitializeApiClient()
    {
        var api = new ApiClientMockup();
        api.RegisterData(typeof(Person), Persons.TestData());
        return api;
    }

    [Fact]
    public void SelectTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            select p;

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectSingleStringFieldTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            select p.Name;

        // Currently not supported
        Assert.Throws<Exception>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void SelectSingleIntegerFieldTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            select p.Age;

        // Currently not supported
        Assert.Throws<Exception>(() =>
        {
            var persons = query.ToList();
        });
    }

    [Fact]
    public void SelectNewSingleFieldTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            select new
            {
                p.Name
            };

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal("name", api.LastRequest?.Fields);
        Assert.Equal(5, persons.Count);
    }
    
    [Fact]
    public void SelectNewNestedSingleFieldIfNullTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            select new
            {
                Manufacturer = p.Car == null ? null : p.Car.Manufacturer
            };

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal("car,car.manufacturer", api.LastRequest?.Fields);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectNewTwoFieldsTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            select new
            {
                p.Name,
                p.Age
            };

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal("name,age", api.LastRequest?.Fields);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectWhereIntGreaterThanTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where p.Age > 26
            select p;

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal("{\"age\": {\"_gt\": 26}}", api.LastRequest.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }
    
    [Fact]
    public void SelectWhereIntNotGreaterThanTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where !(p.Age > 26)
            select p;

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal("{\"age\": {\"_lte\": 26}}", api.LastRequest.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectWhereAndTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where p.Age > 26 && p.Name == "Reid Cantrell"
            select new
            {
                p.Country
            };

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal("{\"_and\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Reid Cantrell\"}}]}",
            api.LastRequest.Filter);
        Assert.Equal("country", api.LastRequest?.Fields);
        
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectWhereOrTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where p.Age > 26 || p.Name == "Chelsey Logan"
            select new
            {
                p.Country,
                p.Name
            };

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal("{\"_or\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Chelsey Logan\"}}]}",
            api.LastRequest.Filter);
        Assert.Equal("country,name", api.LastRequest?.Fields);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectWhereOrTwiceTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where p.Age > 26 || p.Name == "Chelsey Logan" || p.Country == "US"
            select new
            {
                p.Country,
                p.Name
            };

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal(
            "{\"_or\": [{\"_or\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Chelsey Logan\"}}]}, {\"country\": {\"_eq\": \"US\"}}]}",
            api.LastRequest.Filter);
        Assert.Equal("country,name", api.LastRequest?.Fields);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(4, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereStringStartsWithTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where p.Name.StartsWith("Chelsey")
            select p;

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal(
            "{\"name\": {\"_starts_with\": \"Chelsey\"}}",
            api.LastRequest.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereStringEndsWithTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where p.Name.EndsWith("Cantrell")
            select p;

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal(
            "{\"name\": {\"_ends_with\": \"Cantrell\"}}",
            api.LastRequest.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(1, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void WhereStringIsNullOrEmptyTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where string.IsNullOrEmpty(p.Name)
            select p;

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal(
            "{\"name\": {\"_empty\": null}}",
            api.LastRequest.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(0, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    /*
     * TODO it does not work yet
     */
    [Fact]
    public void WhereNestedEqualTest()
    {
        var api = InitializeApiClient();
        var personsSource = api.GetAsQueryable<Person>("person");

        var query =
            from p in personsSource
            where p.Car != null && Equals(p.Car.Model, "Opel")
            select p;

        var persons = query.ToList();
        Assert.Equal("person", api.LastEndpoint);
        Assert.Equal(
            "{\"_and\": [{\"car\": {\"_neq\": null}}, {\"car\": {\"model\": {\"_eq\": \"Opel\"}}}]}",
            api.LastRequest.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(2, persons.Count);
        Assert.Equal(5, persons.Count);
    }
}