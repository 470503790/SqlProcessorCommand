using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>ALTER TABLE ... ADD CONSTRAINT [name] ...</c><br/>
    /// - 通过 <c>sys.objects</c> 判断同名约束是否已存在且隶属于该表；<br/>
    /// - 已存在则跳过；不存在才执行。
    /// </summary>
    internal sealed class AddConstraintTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+ADD\s+CONSTRAINT\s+(?:\[(?<cname>[^\]]+)\]|(?<cname2>\w+))\b(?<rest>[\s\S]*)$",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            var cname  = m.Groups["cname"].Success ? m.Groups["cname"].Value : m.Groups["cname2"].Value;
            return $@"
IF NOT EXISTS (
    SELECT 1 FROM sys.objects WHERE name = N'{SqlId.Unquote(cname)}' AND parent_object_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}')
)
BEGIN
    {block}
END".Trim();
        }
    }
}
