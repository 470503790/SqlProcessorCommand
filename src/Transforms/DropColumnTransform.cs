using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 安全删除列：<c>ALTER TABLE [s].[t] DROP COLUMN [col]</c><br/>
    /// - 仅在该列存在时才执行删除；<br/>
    /// - 适用于 <c>--keep-drop-column</c> 场景。
    /// </summary>
    internal sealed class DropColumnTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+DROP\s+COLUMN\s+(?:\[(?<col>[^\]]+)\]|(?<col2>\w+))\b",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            var col    = m.Groups["col"].Success ? m.Groups["col"].Value : m.Groups["col2"].Value;
            return $@"
IF COL_LENGTH(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}', N'{SqlId.Unquote(col)}') IS NOT NULL
BEGIN
    {block}
END".Trim();
        }
    }
}
