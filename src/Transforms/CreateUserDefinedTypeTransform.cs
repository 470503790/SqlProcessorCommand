using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>CREATE TYPE [schema].[type_name] FROM [base_type]</c> 或 <c>CREATE TYPE ... AS TABLE (...)</c><br/>
    /// - 通过 <c>TYPE_ID('[schema].[type_name]')</c> 判断用户定义类型是否存在；<br/>
    /// - 不存在时才 CREATE。
    /// </summary>
    internal sealed class CreateUserDefinedTypeTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+TYPE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?\[?(?<type>[^\]]+)\]?\s+(?:FROM|AS)",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var typeName = m.Groups["type"].Value;
            
            return $@"
IF TYPE_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(typeName)}') IS NULL
BEGIN
    {block}
END".Trim();
        }
    }
}