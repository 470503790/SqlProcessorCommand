using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>ALTER SEQUENCE [schema].[sequence_name] ...</c><br/>
    /// - 通过 <c>OBJECT_ID('[schema].[sequence_name]', 'SO')</c> 判断序列是否存在；<br/>
    /// - 存在时才 ALTER。适用于 SQL Server 2012+ 的 SEQUENCE 对象。
    /// </summary>
    internal sealed class AlterSequenceTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+SEQUENCE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?\[?(?<sequence>[^\]]+)\]?",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var sequenceName = m.Groups["sequence"].Value;
            
            return $@"
IF OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(sequenceName)}', N'SO') IS NOT NULL
BEGIN
    {block}
END".Trim();
        }
    }
}