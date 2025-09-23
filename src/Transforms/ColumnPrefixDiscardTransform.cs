using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 可选清理器：丢弃以特定“前缀标记”开头的行/块（例如某些脚本生成器的注释头）。<br/>
    /// 默认仅匹配空操作（不触发）；如需启用，请替换 <c>R</c> 的正则以匹配你的标记行。
    /// </summary>
    internal sealed class ColumnPrefixDiscardTransform : ISqlBlockTransform
    {
        private static readonly Regex R = new Regex(@"^\s*--\s*__DISCARD__\b", RegexOptions.IgnoreCase);
        public bool CanHandle(string block) => false; // 默认不启用
        public string Transform(string block) => string.Empty;
    }
}
