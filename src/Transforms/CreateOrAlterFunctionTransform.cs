using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 将 <c>ALTER FUNCTION [s].[f] ...</c> 转换为 SQL Server 2014 兼容写法：<br/>
    /// - 若存在则先 DROP，再以 <c>CREATE FUNCTION</c> 重新创建；<br/>
    /// - 在 DROP 与 CREATE 之间插入 <c>GO</c> 分隔。
    /// </summary>
    internal sealed class CreateOrAlterFunctionTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+FUNCTION\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<name>[^\]]+)\]|(?<name2>\w+))(?=\s|\(|$)",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var name   = m.Groups["name"].Success ? m.Groups["name"].Value : m.Groups["name2"].Value;
            var create = System.Text.RegularExpressions.Regex.Replace(block, @"\bALTER\b", "CREATE", RegexOptions.IgnoreCase);
            return $@"IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(name)}') IS NOT NULL
    DROP FUNCTION {SqlId.Quote(schema)}.{SqlId.Quote(name)};
GO
{create}".Trim();
        }
    }
}
