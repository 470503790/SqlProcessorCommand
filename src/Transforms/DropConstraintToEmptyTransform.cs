using System;
using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 规则：检测到 DROP CONSTRAINT 语句时，直接替换为空（不输出任何内容）。
    /// 示例命中：ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint]
    /// 支持：可选架构名/方括号/末尾分号/大小写不敏感/前后空白
    /// 注意：默认约束（名称以 DF_ 或 DF__ 开头）会被排除
    /// </summary>
    internal sealed class DropConstraintToEmptyTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+DROP\s+CONSTRAINT\s+(?:\[(?<cname>[^\]]+)\]|(?<cname2>\w+))\b",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        /// <summary>是否匹配 DROP CONSTRAINT 语句，但排除默认约束。</summary>
        public bool CanHandle(string block)
        {
            var m = R.Match(block);
            if (!m.Success)
                return false;

            var cname = m.Groups["cname"].Success ? m.Groups["cname"].Value : m.Groups["cname2"].Value;
            
            // 排除默认约束（名称以 DF_ 或 DF__ 开头）
            return !cname.StartsWith("DF_", StringComparison.OrdinalIgnoreCase) &&
                   !cname.StartsWith("DF__", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>匹配后直接返回空串，表示丢弃该语句。</summary>
        public string Transform(string block) => string.Empty;
    }
}
