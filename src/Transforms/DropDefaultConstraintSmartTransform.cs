using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 智能删除默认约束（默认约束名可能未知）：<br/>
    /// - 支持输入：<c>ALTER TABLE [s].[t] DROP CONSTRAINT DF_xxx</c> 或 <c>... DROP DEFAULT FOR [col]</c>；<br/>
    /// - 若仅给出列名，则先查询该列绑定的默认约束名再删除；<br/>
    /// - 此处提供一个简化实现：如果命中 <c>DROP DEFAULT FOR [col]</c>，会解析列并删除绑定的默认约束。
    /// </summary>
    internal sealed class DropDefaultConstraintSmartTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+DROP\s+DEFAULT\s+FOR\s+(?:\[(?<col>[^\]]+)\]|(?<col2>\w+))\b",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            var col    = m.Groups["col"].Success ? m.Groups["col"].Value : m.Groups["col2"].Value;
            return $@"
DECLARE @dcname sysname;
SELECT @dcname = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}')
  AND c.name = N'{SqlId.Unquote(col)}';

IF @dcname IS NOT NULL
BEGIN
    EXEC('ALTER TABLE {SqlId.Quote(schema)}.{SqlId.Quote(table)} DROP CONSTRAINT [' + @dcname + ']');
END".Trim();
        }
    }
}
