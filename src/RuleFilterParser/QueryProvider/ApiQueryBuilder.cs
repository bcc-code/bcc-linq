using System.Text;

namespace RuleFilterParser;

internal class ApiQueryBuilder
{
    public List<string> Filters { get; } = new();
    public string AggregateFunction { get; set; } = "";

    public List<string> Sorts { get; set; } = new();

    public List<string> Expands { get; set; } = new();
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public List<string> Fields { get; set; } = new();

    public Dictionary<string,string> Aliases = new();

    public int Page { get; set; }

    public void Clear()
    {
        Fields.Clear();
        AggregateFunction = string.Empty;
        Sorts.Clear();
        Expands.Clear();
        Limit = null;
        Offset = null;
        Fields.Clear();
        Aliases.Clear();
        Page = 1;
    }
    
    public virtual string GetFilterString()
    {
        if (Filters.Count > 0)
        {
            return Filters.Aggregate((c, n) => $"{{ \"_and\" : [ {c} , {n} ] }}");
        }
        return "";
    }

    [Obsolete]
    public virtual string ToRequestQuery()
    {
        var sb = new StringBuilder();
        // Select all
        sb.Append("");

        // Add filters
        if (Filters.Count > 0)
        {
            sb.Append("&filter=");
            sb.Append(Uri.EscapeDataString(GetFilterString()));
        }

        // Add orderings
        if (Sorts.Count > 0)
        {
            sb.Append("&sort=" + Sorts.Aggregate((c, n) => $"{c},{n}") + "");
        }

        // Add fields
        if (Fields.Count > 0)
        {
            sb.Append("&fields=" + Fields.Select(f => Aliases.TryGetValue(f, out var alias) ? alias : f).Aggregate((c, n) => $"{c},{n}") + "");
        }

        if (Expands.Count > 0)
        {
            if (Fields.Count == 0)
            {
                sb.Append("&fields=*," + Expands.Select(e => $"{e}.*").Aggregate((c, n) => $"{c},{n}") + "");
            }
            else
            {
                sb.Append("," + Expands.Select(e => $"{e}.*").Aggregate((c, n) => $"{c},{n}") + "");
            }
        }

        if (Aliases.Count > 0)
        {
            foreach (var alias in Aliases)
            {
                sb.Append($"&alias[{alias.Key}]={alias.Value}");
            }
        }

        // Add slices
        if (Limit.HasValue && Limit > 0)
        {
            sb.Append($"&limit={Limit}");
        }
        if (Offset.HasValue && Offset > 0)
        {
            sb.Append($"&offset={Offset}");
        }

        return "?" + sb.ToString().TrimStart('&');
    }

    public void MapToApiRequest(IApiRequest request)
    {
        if (Fields.Count > 0)
            request.Fields = Fields.Select(f => Aliases.TryGetValue(f, out var alias) ? alias : f)
                .Aggregate((c, n) => $"{c},{n}");
        else
            request.Fields = "*";
        request.Filter = GetFilterString();
        request.Search = null; // TODO not yet implemented
        request.Sort = string.Join(',', this.Sorts);
        request.Limit = Limit;
        request.Offset = Offset;
        request.Page = Page;
        request.Aggregate = AggregateFunction;
        request.GroupBy = null; // TODO not yet implemented
        request.Deep = null; // TODO not yet implemented
        if (request.Alias != null)
            throw new NotImplementedException();
        request.Alias = null;
        request.Meta = null; // TODO not yet implemented
    }
}