using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    internal static class SqlId
    {
        public static string Quote(string identifier) => identifier == null ? null : "[" + identifier.Replace("]", "]]") + "]";
        public static string Unquote(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return identifier;
            if (identifier.StartsWith("[") && identifier.EndsWith("]")) return identifier.Substring(1, identifier.Length - 2);
            return identifier;
        }
    }
}
