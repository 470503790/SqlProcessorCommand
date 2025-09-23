using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>CREATE SYNONYM [schema].[synonym_name] FOR [target]</c><br/>
    /// - 通过 <c>OBJECT_ID('[schema].[synonym_name]', 'SN')</c> 判断同义词是否存在；<br/>
    /// - 不存在时才 CREATE。
    /// </summary>
    internal sealed class CreateSynonymTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+SYNONYM\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?\[?(?<synonym>[^\]]+)\]?\s+FOR",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var synonymName = m.Groups["synonym"].Value;
            
            return $@"
IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(synonymName)}', N'SN') IS NULL
BEGIN
    {block}
END".Trim();
        }
    }
}