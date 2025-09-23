using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>ALTER TABLE ... ADD CONSTRAINT [DF_xxx] DEFAULT (...) FOR [col]</c><br/>
    /// - 通过 <c>sys.default_constraints</c> + <c>sys.columns</c> 精准判断是否已存在；<br/>
    /// - 若不存在则新增；存在则跳过。
    /// </summary>
    internal sealed class AddDefaultConstraintTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+ADD\s+CONSTRAINT\s+(?:\[(?<df>[^\]]+)\]|(?<df2>\w+))\s+DEFAULT\s+\((?<def>.*?)\)\s+FOR\s+(?:\[(?<col>[^\]]+)\]|(?<col2>\w+))\s*;?\s*$",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            var df     = m.Groups["df"].Success ? m.Groups["df"].Value : m.Groups["df2"].Value;
            var col    = m.Groups["col"].Success ? m.Groups["col"].Value : m.Groups["col2"].Value;
            return $@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.name = N'{SqlId.Unquote(df)}'
      AND dc.parent_object_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}')
      AND c.name = N'{SqlId.Unquote(col)}'
)
BEGIN
    {block}
END".Trim();
        }
    }
}
