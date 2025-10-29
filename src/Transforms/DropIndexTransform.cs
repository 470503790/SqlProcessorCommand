using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 安全删除索引：<c>DROP INDEX [name] ON [schema].[table]</c><br/>
    /// - 仅当该索引存在于目标表时才删除。
    /// </summary>
    internal sealed class DropIndexTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*DROP\s+INDEX\s+(?:\[(?<iname>[^\]]+)\]|(?<iname2>\w+))\s+ON\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))(?=\s|;|$)",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var iname = m.Groups["iname"].Success ? m.Groups["iname"].Value : m.Groups["iname2"].Value;
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            return $@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'{SqlId.Unquote(iname)}' AND object_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}'))
BEGIN
    {block}
END".Trim();
        }
    }
}
