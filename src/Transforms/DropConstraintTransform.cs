using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 安全删除约束：<c>ALTER TABLE ... DROP CONSTRAINT [name]</c><br/>
    /// - 仅当该约束存在且从属于该表时才删除。
    /// </summary>
    internal sealed class DropConstraintTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+DROP\s+CONSTRAINT\s+(?:\[(?<cname>[^\]]+)\]|(?<cname2>\w+))\b",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            var cname  = m.Groups["cname"].Success ? m.Groups["cname"].Value : m.Groups["cname2"].Value;
            return $@"
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'{SqlId.Unquote(cname)}' AND parent_object_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}'))
BEGIN
    {block}
END".Trim();
        }
    }
}
