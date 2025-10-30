using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 丢弃危险语句：<c>DROP USER [user_name]</c><br/>
    /// - 根据配置，直接替换为空（不输出）。
    /// </summary>
    internal sealed class DropUserToEmptyTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*DROP\s+USER\s+(?:\[(?<user>[^\]]+)\]|(?<user2>\w+))",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        public bool CanHandle(string block) => R.IsMatch(block);
        
        public string Transform(string block) => string.Empty;
    }
}
