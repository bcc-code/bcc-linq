using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BccCode.Linq.ApiClient;

namespace RuleToLinqParser.Tests;

public class QueryablePagedEnumerableStateTests
{
    const int BATCH_SIZE = 50;

    [Fact]
    public void PageWithOffsetAndLimit()
    {
        //Assemble
        var clientParams = new QueryableParameters
        {
            Offset = 100,
            Limit = 2000,
            Page = 0
        };
        var state = QueryablePagedEnumerableState.Create(clientParams, BATCH_SIZE);

        // Default 1st iteration
        Assert.Equal(clientParams.Offset, state.PageParameters.Offset);
        Assert.Equal(BATCH_SIZE, state.PageParameters.Limit);
        Assert.Equal(BATCH_SIZE, state.PageSize);
        Assert.False(state.Unlimited);
        Assert.Equal(clientParams.Limit, state.TotalLimit);
        Assert.Equal(0, state.TotalRequested);
        Assert.Null(state.PageParameters.Page);

        var expectedIterations = clientParams.Limit / BATCH_SIZE;
        for (int i = 1; i <= expectedIterations; i++)
        {
            var morePages = state.NextPage(BATCH_SIZE);
            Assert.Equal(i != expectedIterations, morePages);
            if (morePages)
            {
                Assert.Equal(clientParams.Offset + i * BATCH_SIZE, state.PageParameters.Offset);
                Assert.Equal(BATCH_SIZE, state.PageParameters.Limit);
                Assert.Equal(BATCH_SIZE, state.PageSize);
            }
            Assert.False(state.Unlimited);
            Assert.Equal(clientParams.Limit, state.TotalLimit);
            Assert.Equal(i * BATCH_SIZE, state.TotalRequested);
            Assert.Null(state.PageParameters.Page);

        }
        Assert.Equal(clientParams.Limit, state.TotalRequested);
    }

    [Fact]
    public void PageWithLimitLessThanQueryBatchSize()
    {
        //Assemble
        var clientParams = new QueryableParameters
        {
            Offset = 10,
            Limit = 25,
            Page = 0
        };
        var state = QueryablePagedEnumerableState.Create(clientParams, BATCH_SIZE);

        Assert.Equal(clientParams.Offset, state.PageParameters.Offset);
        Assert.Equal(clientParams.Limit, state.PageParameters.Limit);
        Assert.Equal(clientParams.Limit, state.PageSize);
        Assert.False(state.Unlimited);
        Assert.Equal(clientParams.Limit, state.TotalLimit);
        Assert.Equal(0, state.TotalRequested);
        Assert.Null(state.PageParameters.Page);

        Assert.False(state.NextPage(state.PageSize));
        Assert.Equal(state.TotalRequested, clientParams.Limit);
    }

    [Fact]
    public void PageWithLimitNotMultipleOfBatchSize()
    {
        //Assemble
        var clientParams = new QueryableParameters
        {
            Offset = 5,
            Limit = 75
        };
        var state = QueryablePagedEnumerableState.Create(clientParams, BATCH_SIZE);

        // 1st iteration
        Assert.Equal(clientParams.Offset, state.PageParameters.Offset);
        Assert.Equal(BATCH_SIZE, state.PageParameters.Limit);
        Assert.Equal(BATCH_SIZE, state.PageSize);
        Assert.Equal(clientParams.Offset, state.PageParameters.Offset);
        Assert.False(state.Unlimited);
        Assert.Equal(clientParams.Limit, state.TotalLimit);
        Assert.Equal(0, state.TotalRequested);
        Assert.Null(state.PageParameters.Page);

        // 2nd iteration
        Assert.True(state.NextPage(state.PageSize));
        Assert.Equal(clientParams.Limit % BATCH_SIZE, state.PageParameters.Limit);
        Assert.Equal(state.PageParameters.Limit, state.PageSize);
        Assert.Equal(clientParams.Offset + BATCH_SIZE, state.PageParameters.Offset);
        Assert.False(state.Unlimited);
        Assert.Equal(clientParams.Limit, state.TotalLimit);
        Assert.Equal(BATCH_SIZE, state.TotalRequested);

        //3rd iteration
        Assert.False(state.NextPage(state.PageSize));
        Assert.Equal(clientParams.Limit, state.TotalRequested);


    }


    [Fact]
    public void PageWithPageInsteadOfOffset()
    {
        //Assemble
        var clientParams = new QueryableParameters
        {
            Page = 3,
            Limit = 75
        };
        var state = QueryablePagedEnumerableState.Create(clientParams, BATCH_SIZE);

        // 1st iteration
        Assert.Equal(clientParams.Limit * (clientParams.Page - 1), state.PageParameters.Offset);
        Assert.Equal(clientParams.Limit * (clientParams.Page - 1), state.InitialOffset);
        Assert.Equal(BATCH_SIZE, state.PageParameters.Limit);
        Assert.Equal(BATCH_SIZE, state.PageSize);
        Assert.Equal(clientParams.Limit, state.TotalLimit);
        Assert.False(state.Unlimited);
        Assert.Equal(0, state.TotalRequested);
        Assert.Null(state.PageParameters.Page);

        // 2nd iteration
        Assert.True(state.NextPage(state.PageSize));
        Assert.Equal(clientParams.Limit * (clientParams.Page - 1) + BATCH_SIZE, state.PageParameters.Offset);
        Assert.Equal(clientParams.Limit * (clientParams.Page - 1), state.InitialOffset);
        Assert.Equal(state.TotalLimit % BATCH_SIZE, state.PageParameters.Limit);
        Assert.Equal(state.TotalLimit % BATCH_SIZE, state.PageSize);
        Assert.Equal(clientParams.Limit, state.TotalLimit);
        Assert.False(state.Unlimited);
        Assert.Equal(BATCH_SIZE, state.TotalRequested);
        Assert.Null(state.PageParameters.Page);

        //3rd iteration
        Assert.False(state.NextPage(state.PageSize));
        Assert.Equal(clientParams.Limit, state.TotalRequested);
    }

    [Fact]
    public void PageWithLimitAndNoMoreServerResults()
    {
        //Assemble
        var clientParams = new QueryableParameters
        {
            Offset = 0,
            Limit = 500
        };
        var state = QueryablePagedEnumerableState.Create(clientParams, BATCH_SIZE);

        Assert.False(state.Unlimited);
        Assert.True(state.NextPage(BATCH_SIZE));
        Assert.True(state.NextPage(BATCH_SIZE));
        Assert.False(state.NextPage(BATCH_SIZE-1));

    }

    [Fact]
    public void PageWithoutLimitNoMoreServerResults()
    {
        //Assemble
        var clientParams = new QueryableParameters
        {
            Offset = 10
        };
        var state = QueryablePagedEnumerableState.Create(clientParams, BATCH_SIZE);

        Assert.True(state.Unlimited);
        Assert.True(state.NextPage(BATCH_SIZE));
        Assert.True(state.NextPage(BATCH_SIZE));
        Assert.False(state.NextPage(BATCH_SIZE - 1));
    }
}

