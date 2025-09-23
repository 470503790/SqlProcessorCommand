using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>DROP USER [user_name]</c><br/>
    /// - 通过 <c>DATABASE_PRINCIPAL_ID('[user_name]')</c> 判断用户是否存在；<br/>
    /// - 存在时才 DROP。
    /// </summary>
    internal sealed class DropUserTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*DROP\s+USER\s+(?:\[(?<user>[^\]]+)\]|(?<user2>\w+))",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var userName = m.Groups["user"].Success ? m.Groups["user"].Value : m.Groups["user2"].Value;
            
            return $@"
IF DATABASE_PRINCIPAL_ID(N'{SqlId.Quote(userName)}') IS NOT NULL
BEGIN
    {block}
END".Trim();
        }
    }
}