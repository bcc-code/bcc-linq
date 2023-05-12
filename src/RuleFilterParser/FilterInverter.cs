using System.Text.RegularExpressions;
using RuleFilterParser.Exceptions;

namespace RuleFilterParser;

public static class FilterInverter
{
    public static Filter Invert(Filter filter)
    {
        
        var inverted = Activator.CreateInstance(filter.GetType(), "{}") as Filter;

        foreach (var key in filter.Properties.Keys.ToList())
        {
            switch (key)
            {
                case "_and":
                    inverted.Properties.Add("_or",
                        (filter.Properties[key] as Filter)?.GetInvertedFilter() ??
                        throw new ObjectIsNotFilterException());
                    break;
                case "_or":
                    inverted.Properties.Add("_and",
                        (filter.Properties[key] as Filter)?.GetInvertedFilter() ??
                        throw new ObjectIsNotFilterException());
                    break;
                case "_eq":
                    inverted.Properties.Add("_neq", filter.Properties[key]);
                    break;
                case "_neq":
                    inverted.Properties.Add("_eq", filter.Properties[key]);
                    break;
                case "_lt":
                    inverted.Properties.Add("_gte", filter.Properties[key]);
                    break;
                case "_lte":
                    inverted.Properties.Add("_gt", filter.Properties[key]);
                    break;
                case "_gt":
                    inverted.Properties.Add("_lte", filter.Properties[key]);
                    break;
                case "_gte":
                    inverted.Properties.Add("_lt", filter.Properties[key]);
                    break;
                case "_in":
                    inverted.Properties.Add("_nin", filter.Properties[key]);
                    break;
                case "_nin":
                    inverted.Properties.Add("_in", filter.Properties[key]);
                    break;
                case "_null":
                    inverted.Properties.Add("_nnull", filter.Properties[key]);
                    break;
                case "_nnull":
                    inverted.Properties.Add("_null", filter.Properties[key]);
                    break;
                case "_starts_with":
                    inverted.Properties.Add("_nstarts_with", filter.Properties[key]);
                    break;
                case "_nstarts_with":
                    inverted.Properties.Add("_starts_with", filter.Properties[key]);
                    break;
                case "_ends_with":
                    inverted.Properties.Add("_nends_with", filter.Properties[key]);
                    break;
                case "_nends_with":
                    inverted.Properties.Add("_ends_with", filter.Properties[key]);
                    break;
                case "_contains":
                    inverted.Properties.Add("_ncontains", filter.Properties[key]);
                    break;
                case "_ncontains":
                    inverted.Properties.Add("_contains", filter.Properties[key]);
                    break;
                case "_between":
                    inverted.Properties.Add("_nbetween", filter.Properties[key]);
                    break;
                case "_nbetween":
                    inverted.Properties.Add("_between", filter.Properties[key]);
                    break;
                case "_empty":
                    inverted.Properties.Add("_nempty", filter.Properties[key]);
                    break;
                case "_nempty":
                    inverted.Properties.Add("_empty", filter.Properties[key]);
                    break;
                case "_submitted":
                    inverted.Properties.Add("_submitted", !Convert.ToBoolean(filter.Properties[key]));
                    break;
                case "_regex":
                    inverted.Properties.Add("_regex", InvertRegex(filter.Properties[key].ToString() ?? string.Empty));
                    break;
                default:
                    inverted.Properties.Add(key,
                        (filter.Properties[key] as Filter)?.GetInvertedFilter() ??
                        throw new ObjectIsNotFilterException());
                    break;
            }
        }

        return inverted;
    }

    private static string InvertRegex(string pattern)
    {
        var flags = Regex.Match(pattern, @"/([gimuy]*)$").Groups[1].Value;

        pattern = Regex.Replace(pattern, @"/[gimuy]*$", "");
        pattern = pattern.StartsWith("^") ? pattern[1..] : "^" + pattern;

        return flags != "" ? $"/{pattern}/{flags}" : pattern;
    }
}