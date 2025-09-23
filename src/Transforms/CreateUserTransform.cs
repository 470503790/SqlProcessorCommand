using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>CREATE USER [user_name] ...</c><br/>
    /// - 通过 <c>DATABASE_PRINCIPAL_ID('[user_name]')</c> 判断用户是否存在；<br/>
    /// - 不存在时才 CREATE。
    /// </summary>
    internal sealed class CreateUserTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*CREATE\s+USER\s+(?:\[(?<user>[^\]]+)\]|(?<user2>\w+))",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var userName = m.Groups["user"].Success ? m.Groups["user"].Value : m.Groups["user2"].Value;
            
            return $@"
IF DATABASE_PRINCIPAL_ID(N'{SqlId.Quote(userName)}') IS NULL
BEGIN
    {block}
END".Trim();
        }
    }
}