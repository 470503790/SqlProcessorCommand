using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>DROP TYPE [schema].[type_name]</c><br/>
    /// - 通过 <c>TYPE_ID('[schema].[type_name]')</c> 判断用户定义类型是否存在；<br/>
    /// - 存在时才 DROP。
    /// </summary>
    internal sealed class DropUserDefinedTypeTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*DROP\s+TYPE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?\[?(?<type>[^\]]+)\]?",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var typeName = m.Groups["type"].Value;
            
            return $@"
IF TYPE_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(typeName)}') IS NOT NULL
BEGIN
    {block}
END".Trim();
        }
    }
}