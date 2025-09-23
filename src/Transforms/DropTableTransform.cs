using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 安全处理 <c>DROP TABLE [schema].[table]</c>：<br/>
    /// - 默认建议移除（可在上游通过管道策略进行）；<br/>
    /// - 若保留此规则，则改写为“存在才 DROP”。
    /// </summary>
    internal sealed class DropTableTransform : ISqlBlockTransform
    {
        private static readonly System.Text.RegularExpressions.Regex R =
            new System.Text.RegularExpressions.Regex(@"^\s*DROP\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s*;?\s*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            return $@"
IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}', N'U') IS NOT NULL
    {block}".Trim();
        }
    }
}
