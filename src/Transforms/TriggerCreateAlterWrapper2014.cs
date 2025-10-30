using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 2014 兼容的触发器创建包装：对于 <c>CREATE TRIGGER [s].[tr]</c>，<br/>
    /// 先条件 DROP，再 CREATE TRIGGER。
    /// </summary>
    internal sealed class TriggerCreateAlterWrapper2014 : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+TRIGGER\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<name>[^\]]+)\]|(?<name2>\w+))(?=\s|$)",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var name   = m.Groups["name"].Success ? m.Groups["name"].Value : m.Groups["name2"].Value;
            return $@"IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(name)}', N'TR') IS NOT NULL
    DROP TRIGGER {SqlId.Quote(schema)}.{SqlId.Quote(name)};
GO
{block}".Trim();
        }
    }
}
