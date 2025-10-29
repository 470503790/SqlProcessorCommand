using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 规则：检测到 DROP INDEX 语句时，直接替换为空（不输出任何内容）。
    /// 示例命中：DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable]
    /// 支持：可选架构名/方括号/末尾分号/大小写不敏感/前后空白
    /// </summary>
    internal sealed class DropIndexToEmptyTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*DROP\s+INDEX\s+(?:\[(?<iname>[^\]]+)\]|(?<iname2>\w+))\s+ON\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))(?=\s|;|$)",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        /// <summary>是否匹配 DROP INDEX 语句。</summary>
        public bool CanHandle(string block) => R.IsMatch(block);

        /// <summary>匹配后直接返回空串，表示丢弃该语句。</summary>
        public string Transform(string block) => string.Empty;
    }
}
