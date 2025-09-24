using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>DROP ROLE [role_name]</c><br/>
    /// - 通过 <c>DATABASE_PRINCIPAL_ID('[role_name]')</c> 判断角色是否存在；<br/>
    /// - 存在时才 DROP。
    /// </summary>
    internal sealed class DropRoleTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*DROP\s+ROLE\s+(?:\[(?<role>[^\]]+)\]|(?<role2>\w+))",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var roleName = m.Groups["role"].Success ? m.Groups["role"].Value : m.Groups["role2"].Value;
            
            return $@"
IF DATABASE_PRINCIPAL_ID(N'{SqlId.Quote(roleName)}') IS NOT NULL
BEGIN
    {block}
END".Trim();
        }
    }
}