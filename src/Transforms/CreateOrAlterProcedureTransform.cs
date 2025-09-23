using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 将 <c>CREATE OR ALTER PROCEDURE [s].[p] ...</c> 转换为 SQL Server 2014 兼容写法：<br/>
    /// - 若存在则先 DROP，再以 <c>CREATE PROCEDURE</c> 重新创建；<br/>
    /// - 在 DROP 与 CREATE 之间插入 <c>GO</c> 分隔。
    /// </summary>
    internal sealed class CreateOrAlterProcedureTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+OR\s+ALTER\s+PROCEDURE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<name>[^\]]+)\]|(?<name2>\w+))\b",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var name   = m.Groups["name"].Success ? m.Groups["name"].Value : m.Groups["name2"].Value;
            var create = System.Text.RegularExpressions.Regex.Replace(block, @"\bCREATE\s+OR\s+ALTER\b", "CREATE", RegexOptions.IgnoreCase);
            return $@"IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(name)}', N'P') IS NOT NULL
    DROP PROCEDURE {SqlId.Quote(schema)}.{SqlId.Quote(name)};
GO
{create}".Trim();
        }
    }
}
