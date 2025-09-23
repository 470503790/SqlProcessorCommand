using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    internal static class Rx
    {
        public static Match M(string input, string pattern) =>
            Regex.Match(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public static bool Is(string input, string pattern) => M(input, pattern).Success;
    }
}
