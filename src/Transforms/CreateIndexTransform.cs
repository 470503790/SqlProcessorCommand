using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>CREATE [UNIQUE] [NONCLUSTERED|CLUSTERED] INDEX [name] ON [schema].[table](...)</c><br/>
    /// - 通过 <c>sys.indexes</c> 判断索引名是否存在于目标表；不存在才创建。
    /// </summary>
    internal sealed class CreateIndexTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+(?:UNIQUE\s+)?(?:NONCLUSTERED|CLUSTERED)?\s*INDEX\s+(?:\[(?<iname>[^\]]+)\]|(?<iname2>\w+))\s+ON\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s*\(",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var iname = m.Groups["iname"].Success ? m.Groups["iname"].Value : m.Groups["iname2"].Value;
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            return $@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'{SqlId.Unquote(iname)}' AND object_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}'))
BEGIN
    {block}
END".Trim();
        }
    }
}
