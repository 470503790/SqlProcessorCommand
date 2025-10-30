using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 2014 兼容的视图创建包装：对于 <c>CREATE VIEW [s].[v]</c>，先条件 DROP，再 CREATE VIEW。
    /// </summary>
    internal sealed class ViewCreateAlterWrapper2014 : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+VIEW\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<name>[^\]]+)\]|(?<name2>\w+))(?=\s|$)",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var name   = m.Groups["name"].Success ? m.Groups["name"].Value : m.Groups["name2"].Value;
            return $@"IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(name)}', N'V') IS NOT NULL
    DROP VIEW {SqlId.Quote(schema)}.{SqlId.Quote(name)};
GO
{block}".Trim();
        }
    }
}
