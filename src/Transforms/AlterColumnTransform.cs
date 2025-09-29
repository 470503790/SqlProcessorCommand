using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>ALTER TABLE [schema].[table] ALTER COLUMN [col] ...</c><br/>
    /// - 仅在目标列存在时才尝试 ALTER；<br/>
    /// - 由于解析完整类型定义较复杂，默认直接执行（重复执行通常无害）；<br/>
    /// - 可根据需要进一步增强成"只在类型/可空性不匹配时才执行"。
    /// </summary>
    internal sealed class AlterColumnTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+ALTER\s+COLUMN\s+(?:\[(?<col>[^\]]+)\]|(?<col2>\w+))(?<rest>[\s\S]*)$",
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
