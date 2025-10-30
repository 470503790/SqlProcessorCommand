using System;
using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 安全删除约束：<c>ALTER TABLE ... DROP CONSTRAINT [name]</c><br/>
    /// - 仅当该约束存在且从属于该表时才删除。
    /// <para>
    /// <strong>为什么排除默认约束（DF_/DF__开头）？</strong><br/>
    /// 默认约束需要特殊处理，由 <see cref="DropDefaultConstraintSmartTransform"/> 单独处理。
    /// 该转换器支持智能删除默认约束，即使约束名未知（例如：ALTER TABLE ... DROP DEFAULT FOR [column]），
    /// 会自动查询系统表找到绑定的默认约束名后再删除。因此，名称以 DF_ 或 DF__ 开头的约束
    /// 会被本转换器排除，交由专门的默认约束处理器处理，避免冲突并确保正确处理。
    /// </para>
    /// </summary>
    internal sealed class DropConstraintTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\])\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+DROP\s+CONSTRAINT\s+(?:\[(?<cname>[^\]]+)\]|(?<cname2>\w+))(?=\s|;|$)",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        /// <summary>
        /// 是否匹配 DROP CONSTRAINT 语句，但排除默认约束。
        /// <para>
        /// 排除默认约束的原因：默认约束（名称通常以 DF_ 或 DF__ 开头）由 
        /// <see cref="DropDefaultConstraintSmartTransform"/> 专门处理，该转换器可以智能地
        /// 通过列名查询并删除默认约束，即使约束名未知。将默认约束从此处排除，
        /// 避免重复处理和冲突。
        /// </para>
        /// </summary>
        public bool CanHandle(string block)
        {
            var m = R.Match(block);
            if (!m.Success)
                return false;

            var cname = m.Groups["cname"].Success ? m.Groups["cname"].Value : m.Groups["cname2"].Value;
            
            // 排除默认约束（名称以 DF_ 或 DF__ 开头）
            // 原因：默认约束由 DropDefaultConstraintSmartTransform 单独处理
            return !cname.StartsWith("DF_", StringComparison.OrdinalIgnoreCase) &&
                   !cname.StartsWith("DF__", StringComparison.OrdinalIgnoreCase);
        }
        
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : "dbo";
            var table  = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            var cname  = m.Groups["cname"].Success ? m.Groups["cname"].Value : m.Groups["cname2"].Value;
            return $@"
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'{SqlId.Unquote(cname)}' AND parent_object_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}'))
BEGIN
    {block}
END".Trim();
        }
    }
}
