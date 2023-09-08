using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using BccCode.ApiClient;
using Microsoft.AspNetCore.Mvc;

namespace BccCode.ApiClient
{
    public class ApiQueryParameters : IApiQueryParameters
    {
        
        /// <summary>
        /// Choose the fields that are returned in the current dataset. This parameter supports dot notation to request
        /// nested relational fields. You can also use a wildcard (*) to include all fields at a specific depth.
        /// </summary>
        [FromQuery(Name = "fields")]
        public string? Fields { get; set; }

        /// <summary>
        /// Used to search items in a collection that matches the filter's conditions. The filter param follows the Filter
        /// Rules spec, which includes additional information on logical operators (AND/OR), nested relational filtering,
        /// and dynamic variables.
        /// </summary>
        [FromQuery(Name = "filter")]
        public string? Filter { get; set; }

        /// <summary>
        /// The search parameter allows you to perform a search on all string and text type fields within a collection.
        /// It's an easy way to search for an item without creating complex field filters – though it is far less optimized.
        /// It only searches the root item's fields, related item fields are not included.
        /// </summary>
        [FromQuery(Name = "search")]
        public string? Search { get; set; }

        /// <summary>
        /// What field(s) to sort by. Sorting defaults to ascending, but a minus sign (<c>-</c>) can be used to reverse this to
        /// descending order. Fields are prioritized by the order in the parameter. The dot-notation has to be used when
        /// sorting with values of nested fields.
        /// </summary>
        [FromQuery(Name = "sort")]
        public string? Sort { get; set; }

        /// <summary>
        /// Set the maximum number of items that will be returned. The default limit is set to <c>100</c>.
        /// </summary>
        [FromQuery(Name = "limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Skip the first <c>n</c> items in the response. Can be used for pagination.
        /// </summary>
        [FromQuery(Name = "offset")]
        public int? Offset { get; set; }

        /// <summary>
        /// An alternative to <c>offset</c>. Page is a way to set <c>offset</c> under the hood by calculating
        /// <c>limit * page</c>. Page is 1-indexed.
        /// </summary>
        [FromQuery(Name = "page")]
        public int? Page { get; set; }

        /// <summary>
        /// Aggregate functions allow you to perform calculations on a set of values, returning a single result.
        /// 
        /// The following aggregation functions are available in Directus:
        ///
        /// <list type="table">
        ///   <listheader>
        ///     <term>Name</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term><c>count</c></term>
        ///     <description>Counts how many items there are</description>
        ///   </item>
        ///   <item>
        ///     <term><c>countDistinct</c></term>
        ///     <description>Counts how many unique items there are</description>
        ///   </item>
        ///   <item>
        ///     <term><c>sum</c></term>
        ///     <description>Adds together the values in the given field</description>
        ///   </item>
        ///   <item>
        ///     <term><c>sumDistinct</c></term>
        ///     <description>Adds together the unique values in the given field</description>
        ///   </item>
        ///   <item>
        ///     <term><c>avg</c></term>
        ///     <description>Get the average value of the given field</description>
        ///   </item>
        ///   <item>
        ///     <term><c>avgDistinct</c></term>
        ///     <description>Get the average value of the unique values in the given field</description>
        ///   </item>
        ///   <item>
        ///     <term><c>min</c></term>
        ///     <description>Return the lowest value in the field</description>
        ///   </item>
        ///   <item>
        ///     <term><c>max</c></term>
        ///     <description>Return the highest value in the field</description>
        ///   </item>
        /// </list>
        /// </summary>
        [FromQuery(Name = "aggregate")]
        public string? Aggregate { get; set; }

        /// <summary>
        /// By default, the above aggregation functions run on the whole dataset. To allow for more flexible reporting,
        /// you can combine the above aggregation with grouping. Grouping allows for running the aggregation functions
        /// based on a shared value. This allows for things like "Average rating per month" or "Total sales of items in
        /// the jeans category".
        ///
        /// The <c>groupBy</c> query allows for grouping on multiple fields simultaneously. Combined with the Functions,
        /// this allows for aggregate reporting per year-month-date.
        /// </summary>
        [FromQuery(Name = "groupBy")]
        public string? GroupBy { get; set; }

        /// <summary>
        /// Deep allows you to set any of the other query parameters on a nested relational dataset.
        /// </summary>
        [FromQuery(Name = "deep")]
        public string? Deep { get; set; }

        /// <summary>
        /// Aliases allow you rename fields on the fly, and request the same nested data set multiple times using different
        /// filters.
        ///
        /// <remarks>
        /// <b>Nested fields</b>
        /// It is only possible to alias same level fields.
        /// Alias for nested fields, f.e. <c>field.nested</c>, will not work.
        /// </remarks>
        /// </summary>
        [FromQuery(Name = "alias")]
        public string? Alias { get; set; }

        /// <summary>
        /// Metadata allows you to retrieve some additional information about the items in the collection you're fetching.
        /// <c>*</c> can be used as a wildcard to retrieve all metadata.
        ///
        /// <b>Total Count (<c>total_count</c>)</b>
        /// Returns the total item count of the collection you're querying.
        ///
        /// <b>Filter Count (<c>filter_count</c>)</b>
        /// </summary>
        [FromQuery(Name = "meta")]
        public string? Meta { get; set; }

    }
}
