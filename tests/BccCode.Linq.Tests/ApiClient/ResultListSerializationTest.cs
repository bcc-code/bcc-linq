using BccCode.Linq.Tests.Helpers;
using BccCode.Platform;
using Newtonsoft.Json;

namespace BccCode.Linq.Tests.ApiClient;

public class ResultListSerializationTest
{
    [Fact]
    public void SerializeSingleWithMetadataText()
    {
        var resultList = new ResultList<Person>
        {
            Data = new List<Person>
            {
                new("Chuck Norris", 0, "US", new DateTime(1940, 3, 10))
                {
                    Car = new("Opel", "Astra", 2003)
                }
            },
            Meta = new Metadata
            {
                {"total", (object)1}
            }
        };


        resultList.Data.ForEach(d =>
        {
            d.CarHistory = null;
            d.Car.ManufacturerInfo = null;
        });

        var jsonString = JsonConvert.SerializeObject(resultList);

        Assert.Equal(@"{""data"":[{""Name"":""Chuck Norris"",""Age"":0,""Country"":""US"",""CarHistory"":null,""Car"":{""Manufacturer"":""Opel"",""Model"":""Astra"",""YearOfProduction"":2003,""ManufacturerInfo"":null},""AnyDate"":""1940-03-10T00:00:00"",""Type"":0}],""meta"":{""total"":1}}", jsonString);
    }

    [Fact]
    public void SerializeSingleNoMetadataText()
    {
        var resultList = new ResultList<Person>
        {
            Data = new List<Person>
            {
                new("Chuck Norris", 0, "US", new DateTime(1940, 3, 10))
                {
                    Car = new("Opel", "Astra", 2003)
                }
            }
        };

        resultList.Data.ForEach(d =>
        {
            d.CarHistory = null;
            d.Car.ManufacturerInfo = null;
        });

        var jsonString = JsonConvert.SerializeObject(resultList);

        Assert.Equal(@"{""data"":[{""Name"":""Chuck Norris"",""Age"":0,""Country"":""US"",""CarHistory"":null,""Car"":{""Manufacturer"":""Opel"",""Model"":""Astra"",""YearOfProduction"":2003,""ManufacturerInfo"":null},""AnyDate"":""1940-03-10T00:00:00"",""Type"":0}]}", jsonString);
    }

    [Fact]
    public void SerializeEmptyWithMetadataText()
    {
        var resultList = new ResultList<Person>
        {
            Data = new List<Person>(),
            Meta = new Metadata
            {
                {"total", (object)0}
            }
        };

        var jsonString = JsonConvert.SerializeObject(resultList);

        Assert.Equal(@"{""data"":[],""meta"":{""total"":0}}", jsonString);
    }

    [Fact]
    public void SerializeEmptyNoMetadataText()
    {
        var resultList = new ResultList<Person>
        {
            Data = new List<Person>()
        };

        var jsonString = JsonConvert.SerializeObject(resultList);

        Assert.Equal(@"{""data"":[]}", jsonString);
    }

    [Fact]
    public void DeserializeEmptyResultWithMetadataTest()
    {
        var resultList = JsonConvert.DeserializeObject<ResultList<Person>>(@"
{
  ""data"": [],
  ""meta"": {
    ""total"": 0
  }
}");
        Assert.NotNull(resultList);
        Assert.NotNull(resultList?.Data);
        Assert.Empty(resultList?.Data);
        Assert.Equal(0, resultList?.Data?.Count);
        Assert.Equal(0, resultList?.GetData()?.Count());
        Assert.NotNull(resultList?.Meta);
        Assert.Equal((long)0, resultList?.Meta["total"]);
        Assert.Equal(0, resultList?.Meta?.Total);
        Assert.Equal(0, resultList?.Meta?.FilterCount);
    }

    [Fact]
    public void DeserializeEmptyResultNoMetadataTest()
    {
        var resultList = JsonConvert.DeserializeObject<ResultList<Person>>(@"
{
  ""data"": []
}");
        Assert.NotNull(resultList);
        Assert.NotNull(resultList.Data);
        Assert.Empty(resultList.Data);
        Assert.Null(resultList.Meta);
    }

    [Fact]
    public void DeserializeOnePersonResultNoMetadataTest()
    {
        var resultList = JsonConvert.DeserializeObject<ResultList<Person>>(@"
{
  ""data"": [
    {
      ""Name"": ""Chuck Norris""
    }
  ]
}");
        Assert.NotNull(resultList);
        Assert.NotNull(resultList.Data);
        Assert.Single(resultList.Data);
        Assert.Equal("Chuck Norris", resultList.Data[0].Name);
        Assert.Null(resultList.Meta);
    }

    [Fact]
    public void DeserializeOnePersonResultWithMetadataTest()
    {
        var resultList = JsonConvert.DeserializeObject<ResultList<Person>>(@"
{
  ""data"": [
    {
      ""Name"": ""Chuck Norris""
    }
  ],
  ""meta"": {
    ""filter_count"": 1
  }
}");
        Assert.NotNull(resultList);
        Assert.NotNull(resultList.Data);
        Assert.Single(resultList.Data);
        Assert.Equal("Chuck Norris", resultList.Data[0].Name);
        Assert.NotNull(resultList.Meta);
        Assert.Equal((long)1, resultList.Meta["filter_count"]);
        Assert.Equal(1, resultList.Meta.Total);
        Assert.Equal(1, resultList.Meta.FilterCount);
    }
}
