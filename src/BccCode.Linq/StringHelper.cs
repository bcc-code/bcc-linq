namespace BccCode.Linq;

internal static class StringHelper
{
    /// <summary>
    /// Transforms a pascal case string into camel case string.
    /// </summary>
    /// <param name="str">
    /// String in pascal case. E.g. <c>"HelloWorld"</c>.
    /// </param>
    /// <returns>
    /// String in camel case. E.g. <c>"hellWorld"</c>.
    /// </returns>
    internal static string ToCamelCase(this string str)
    {
        //if (ReferenceEquals(str, null)) return null;
        if (str.Length == 0) return str;

        if (str.Length == 1) return str.ToLower();

        //Make first letter lowercase (i.e. camelCase)
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
