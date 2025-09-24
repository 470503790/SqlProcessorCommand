using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>DROP SCHEMA [schema_name]</c><br/>
    /// - 通过 <c>SCHEMA_ID('[schema_name]')</c> 判断架构是否存在；<br/>
    /// - 存在时才 DROP。
    /// </summary>
    internal sealed class DropSchemaTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*DROP\s+SCHEMA\s+(?:\[(?<schema>[^\]]+)\]|(?<schema2>\w+))",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : m.Groups["schema2"].Value;
            
            return $@"
IF SCHEMA_ID(N'{SqlId.Quote(schema)}') IS NOT NULL
BEGIN
    {block}
END".Trim();
        }
    }
}