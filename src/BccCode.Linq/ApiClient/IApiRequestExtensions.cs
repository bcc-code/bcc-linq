using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Policy;
using System.Text;
using BccCode.ApiClient;

namespace BccCode.Linq.ApiClient
{
    public static class IApiRequestExtensions
    {
        public static string ToQueryString(this IApiRequest request)
        {            
            var urlParameters = new Dictionary<string, string>();
            if (request.Fields != null)
                urlParameters.Add("fields", request.Fields);
            if (request.Filter != null)
                urlParameters.Add("filter", request.Filter);
            if (request.Search != null)
                urlParameters.Add("search", request.Search);
            if (request.Sort != null)
                urlParameters.Add("sort", request.Sort);
            if (request.Limit.HasValue)
                urlParameters.Add("limit", request.Limit.Value.ToString(CultureInfo.InvariantCulture));
            if (request.Offset.HasValue)
                urlParameters.Add("offset", request.Offset.Value.ToString(CultureInfo.InvariantCulture));
            if (request.Page.HasValue)
                urlParameters.Add("page", request.Page.Value.ToString(CultureInfo.InvariantCulture));
            if (request.Aggregate != null)
                urlParameters.Add("aggregate", request.Aggregate);
            if (request.GroupBy != null)
                urlParameters.Add("groupBy", request.GroupBy);
            if (request.Deep != null)
                urlParameters.Add("deep", request.Deep);
            if (request.Alias != null)
                urlParameters.Add("alias", request.Alias);
            if (request.Meta != null)
                urlParameters.Add("meta", request.Meta);

            return string.Join('&', urlParameters.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));            
        }

    }
}
