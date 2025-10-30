using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 将 <c>CREATE OR ALTER TRIGGER [s].[tr] ...</c> 转换为 2014 兼容写法：存在则 DROP TRIGGER，再 CREATE TRIGGER。
    /// </summary>
    internal sealed class CreateOrAlterTriggerTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+OR\s+ALTER\s+TRIGGER\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<name>[^\]]+)\]|(?<name2>\w+))(?=\s|$)",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var name   = m.Groups["name"].Success ? m.Groups["name"].Value : m.Groups["name2"].Value;
            var create = System.Text.RegularExpressions.Regex.Replace(block, @"\bCREATE\s+OR\s+ALTER\b", "CREATE", RegexOptions.IgnoreCase);
            return $@"IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(name)}', N'TR') IS NOT NULL
    DROP TRIGGER {SqlId.Quote(schema)}.{SqlId.Quote(name)};
GO
{create}".Trim();
        }
    }
}
