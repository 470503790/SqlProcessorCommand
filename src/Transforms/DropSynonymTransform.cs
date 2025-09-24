using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>DROP SYNONYM [schema].[synonym_name]</c><br/>
    /// - 通过 <c>OBJECT_ID('[schema].[synonym_name]', 'SN')</c> 判断同义词是否存在；<br/>
    /// - 存在时才 DROP。
    /// </summary>
    internal sealed class DropSynonymTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*DROP\s+SYNONYM\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?\[?(?<synonym>[^\]]+)\]?",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var synonymName = m.Groups["synonym"].Value;
            
            return $@"
IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(synonymName)}', N'SN') IS NOT NULL
BEGIN
    {block}
END".Trim();
        }
    }
}