namespace BccCode.Linq.Tests;

public class LinqQueryProviderTests
{
    #region Select

    [Fact]
    public void SelectTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal("*", api.LastRequest?.Fields ?? "*");
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
        Assert.Equal(5, persons.Count);
    }

    [Fact]
    public void SelectSingleStringFieldTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select p.Name;

        // Currently not supported
        Assert.Throws<Exception>(() =>
        {
            // ReSharper disable once UnusedVariable
            var persons = query.ToList();
        });
    }

    [Fact]
    public void SelectSingleIntegerFieldTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            select p.Age;

        // Currently not supported
        Assert.Throws<Exception>(() =>
        {
            // ReSharper disable once UnusedVariable
            var persons = query.ToList();
        });
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal("name", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
                Manufacturer = p.Car == null ? null : p.Car.Manufacturer
            };

        var persons = query.ToList();
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal("car,car.manufacturer", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal("name,age", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
        Assert.Equal(5, persons.Count);
    }

    #endregion

    #region Where

    [Fact]
    public void WhereIntGreaterThanTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            where p.Age > 26
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal("{\"age\": {\"_gt\": 26}}", api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal("{\"age\": {\"_lte\": 26}}", api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal("{\"_and\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Reid Cantrell\"}}]}",
            api.LastRequest?.Filter);
        Assert.Equal("country", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal("{\"_or\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Chelsey Logan\"}}]}",
            api.LastRequest?.Filter);
        Assert.Equal("country,name", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal(
            "{\"_or\": [{\"_or\": [{\"age\": {\"_gt\": 26}}, {\"name\": {\"_eq\": \"Chelsey Logan\"}}]}, {\"country\": {\"_eq\": \"US\"}}]}",
            api.LastRequest?.Filter);
        Assert.Equal("country,name", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal(
            "{\"name\": {\"_starts_with\": \"Chelsey\"}}",
            api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal(
            "{\"name\": {\"_ends_with\": \"Cantrell\"}}",
            api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal(
            "{\"name\": {\"_empty\": null}}",
            api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Where clauses. Since we remove the Where clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(0, persons.Count);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Equal(
            "{\"_and\": [{\"car\": {\"_neq\": null}}, {\"car\": {\"model\": {\"_eq\": \"Opel\"}}}]}",
            api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Null(api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields ?? "*");
        Assert.Equal("name", api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Null(api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Equal("-name", api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Null(api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields ?? "*");
        Assert.Equal("name,-age,country", api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
        Assert.Equal(5, persons.Count);
    }

    /// <summary>
    /// We do by design not support filtering on nesting columns
    /// </summary>
    [Fact]
    public void OrderByNestingNotSupportedTest()
    {
        var api = new ApiClientMockup();

        var query =
            from p in api.Persons
            orderby p.Car.Manufacturer
            select p;

        var persons = query.ToList();
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Null(api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields ?? "*");
        Assert.Equal("car.manufacturer", api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Null(api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Equal(2, api.LastRequest?.Offset);
        Assert.Null(api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Null(api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Null(api.LastRequest?.Offset);
        Assert.Equal(3, api.LastRequest?.Limit);
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
        Assert.Equal("persons", api.LastEndpoint);
        Assert.Null(api.LastRequest?.Filter);
        Assert.Equal("*", api.LastRequest?.Fields);
        Assert.Null(api.LastRequest?.Sort);
        Assert.Equal(2, api.LastRequest?.Offset);
        Assert.Equal(3, api.LastRequest?.Limit);
        // NOTE: Currently the Mockup API Client does not interpret Take clauses. Since we remove the Take clause
        //       from the expression tree, the result will be still the total count of the mockup data.
        //Assert.Equal(3, persons.Count);
        Assert.Equal(5, persons.Count);
    }

    #endregion
}
