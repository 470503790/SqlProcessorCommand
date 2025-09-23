using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlProcessorCommand.src.Transforms
{
    /// <summary>
    /// 规则：检测到 DROP TABLE 语句时，直接替换为空（不输出任何内容）。
    /// 示例命中：DROP TABLE [dbo].[f_statement_bill_item]
    /// 支持：可选架构名/方括号/末尾分号/大小写不敏感/前后空白
    /// </summary>
    internal sealed class DropTableToEmptyTransform : ISqlBlockTransform
    {
        private static readonly Regex R = new Regex(
            @"^\s*DROP\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s*;?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant
        );

        /// <summary>是否匹配 DROP TABLE 语句。</summary>
        public bool CanHandle(string block) => R.IsMatch(block);

        /// <summary>匹配后直接返回空串，表示丢弃该语句。</summary>
        public string Transform(string block) => string.Empty;
    }
}
