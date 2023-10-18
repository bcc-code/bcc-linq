
using BccCode.Platform.Apis;

namespace BccCode.Linq.Client;


public class QueryablePagedEnumerableState
{
    private QueryablePagedEnumerableState() { PageParameters = new QueryableParameters(); ClientParameters = new QueryableParameters(); }

    private QueryablePagedEnumerableState(IQueryableParameters parameters, int queryBatchSize)
    {
        ClientParameters = parameters;
        PageParameters = parameters.Clone();
        QueryBatchSize = queryBatchSize;
    }
    public static QueryablePagedEnumerableState Create(IQueryableParameters clientParams, int queryBatchSize)
    {
        var s = new QueryablePagedEnumerableState(clientParams, queryBatchSize);

        // Set total limit to Limit provided by client
        s.TotalLimit = clientParams.Limit;

        // Calculate Offset if Page has been specified instead of Offset
        if (clientParams.Page.HasValue && clientParams.Page > 0)
        {
            // If Page is specified by the client, we only want to request one page.
            s.TotalLimit = clientParams.Limit ?? s.QueryBatchSize;

            // Calculate offset from Page and Limit
            var offset = (clientParams.Page.Value - 1) * s.TotalLimit;

            // Validation: Check if offset has been set to something other than offset calculated from Page
            if (clientParams.Offset.HasValue && clientParams.Offset > 0 && clientParams.Offset.Value != offset)
            {
                throw new Exception("Offset and Page parameters cannot be used simultaneously. If Page is set, offset will be calculated using the formula Offset=(Page-1)*Limit.");
            }

            //Set Offset instead of Page
            s.PageParameters.Offset = offset;
        }
        //Don't use Page parameter for server requests
        s.PageParameters.Page = null;
        s.InitialOffset = s.PageParameters.Offset ?? 0;
        s.PageSize = s.Unlimited ? s.QueryBatchSize : Math.Min(s.TotalLimit!.Value, s.QueryBatchSize);
        s.PageParameters.Limit = s.PageSize;
        s.NextPage(0);
        return s;
    }

    public int QueryBatchSize { get; }

    public bool Unlimited => !TotalLimit.HasValue || TotalLimit == 0;

    public int? TotalLimit { get; private set; }

    public int InitialOffset { get; private set; }

    public int TotalRequested { get; private set; }

    public int PageSize { get; private set; }

    public IQueryableParameters ClientParameters { get; }

    public IQueryableParameters PageParameters { get; }

    public bool NextPage(int previousResultCount)
    {
        TotalRequested += previousResultCount;
        PageParameters.Offset = InitialOffset + TotalRequested;

        //Avoid sending redundant parameter
        if (PageParameters.Offset == 0)
        {
            PageParameters.Offset = null;
        }

        PageSize = GetNextPageSize();
        PageParameters.Limit = PageSize;


        if (previousResultCount < PageSize)
        {
            return false;
        }
        if (!Unlimited && TotalRequested >= TotalLimit)
        {
            return false;
        }

        return true;
    }

    private int GetNextPageSize()
    {
        if (!Unlimited && TotalLimit < TotalRequested + PageSize)
        {
            return TotalLimit!.Value - TotalRequested;
        }
        return PageSize;
    }


}

