using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 规则：针对“表级”扩展属性的 <c>EXEC sp_addextendedproperty</c>（位置参数写法），
    /// 生成 SQL Server 2014 兼容的幂等包装：存在则 UPDATE，不存在则 ADD。
    /// 示例（表级，无列）：
    /// <code>
    /// EXEC sp_addextendedproperty
    /// 'MS_Description', N'工厂图片表',
    /// 'SCHEMA', N'dbo',
    /// 'TABLE',  N'p_factory_pics'
    /// </code>
    /// 匹配要点：大小写不敏感，允许多行与可选分号。
    /// </summary>
    internal sealed class ExtendedPropertyAddOrUpdateTablePositionalTransform : ISqlBlockTransform
    {
        private static readonly Regex R = new Regex(
            // EXEC/EXECUTE + 可选 sys. 前缀
            @"^\s*EXEC(?:UTE)?\s+(?:sys\.)?sp_addextendedproperty\s+" +
            // 参数1：@name 或 位置参数 'name'
            @"(?:(?:@name\s*=\s*)?N?'(?<pname>[^']+)')\s*,\s*" +
            // 参数2：@value 或 位置参数 N'value'
            @"(?:(?:@value\s*=\s*)?N?'(?<pvalue>[^']*)')\s*,\s*" +
            // 参数3-6：SCHEMA/表
            @"(?:(?:@level0type\s*=\s*)?N?'SCHEMA')\s*,\s*" +
            @"(?:(?:@level0name\s*=\s*)?N?'(?<schema>[^']+)')\s*,\s*" +
            @"(?:(?:@level1type\s*=\s*)?N?'TABLE')\s*,\s*" +
            @"(?:(?:@level1name\s*=\s*)?N?'(?<table>[^']+)')\s*;?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant
        );

        public bool CanHandle(string block) => R.IsMatch(block);

        public string Transform(string block)
        {
            var m = R.Match(block);
            var pname = m.Groups["pname"].Value;
            var schema = m.Groups["schema"].Value;
            var table = m.Groups["table"].Value;

            // 生成对应的 ADD 与 UPDATE 版本（保持原有参数风格/换行）
            var addStmt = Regex.Replace(block, @"\bsp_updateextendedproperty\b", "sp_addextendedproperty", RegexOptions.IgnoreCase);
            var updateStmt = Regex.Replace(block, @"\bsp_addextendedproperty\b", "sp_updateextendedproperty", RegexOptions.IgnoreCase);

            // 表级扩展属性存在性：class=1, major_id=表, minor_id=0
            var existsExpr =
                $"ep.class = 1 AND ep.major_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}') AND ep.minor_id = 0";

            return $@"
IF EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.name = N'{pname}' AND {existsExpr})
BEGIN
    {updateStmt.Trim()}
END
ELSE
BEGIN
    {addStmt.Trim()}
END".Trim();
        }
    }
}
