using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 丢弃危险语句：<c>ALTER TABLE ... DROP COLUMN ...</c><br/>
    /// - 根据配置，默认直接替换为空（不输出）。
    /// </summary>
    internal sealed class DropColumnToEmptyTransform : ISqlBlockTransform
    {
        private static readonly System.Text.RegularExpressions.Regex R =
            new System.Text.RegularExpressions.Regex(@"^\s*ALTER\s+TABLE\s+[\s\S]*?\bDROP\s+COLUMN\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block) => string.Empty;
    }
}
