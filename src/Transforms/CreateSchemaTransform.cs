using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>CREATE SCHEMA [schema_name]</c><br/>
    /// - 通过 <c>SCHEMA_ID('[schema_name]')</c> 判断架构是否存在；<br/>
    /// - 不存在时才 CREATE。
    /// </summary>
    internal sealed class CreateSchemaTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+SCHEMA\s+(?:\[(?<schema>[^\]]+)\]|(?<schema2>\w+))(?:\s+AUTHORIZATION\s+(?:\[(?<owner>[^\]]+)\]|(?<owner2>\w+)))?",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : m.Groups["schema2"].Value;
            
            return $@"
IF SCHEMA_ID(N'{SqlId.Quote(schema)}') IS NULL
BEGIN
    {block}
END".Trim();
        }
    }
}