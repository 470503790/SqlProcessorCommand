using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 2014 兼容的函数创建包装：对于 <c>CREATE FUNCTION [s].[f]</c>，<br/>
    /// 先条件 DROP（FN/IF/TF 均可），再 CREATE FUNCTION，避免重复创建。
    /// </summary>
    internal sealed class FunctionCreateAlterWrapper2014 : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+FUNCTION\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<name>[^\]]+)\]|(?<name2>\w+))\b",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var name   = m.Groups["name"].Success ? m.Groups["name"].Value : m.Groups["name2"].Value;
            return $@"IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(name)}') IS NOT NULL
    DROP FUNCTION {SqlId.Quote(schema)}.{SqlId.Quote(name)};
GO
{block}".Trim();
        }
    }
}
