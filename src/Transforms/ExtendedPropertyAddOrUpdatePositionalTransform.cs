using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 规则：将 <c>EXEC sp_addextendedproperty</c> 的“位置参数”写法（如示例）
    /// 转换为“存在则 UPDATE，不存在则 ADD”的幂等逻辑。支持大小写不敏感、换行与空白。
    /// 示例：
    /// <code>
    /// EXEC sp_addextendedproperty
    /// 'MS_Description', N'ID',
    /// 'SCHEMA', N'dbo',
    /// 'TABLE', N'p_factory_pics',
    /// 'COLUMN', N'id'
    /// </code>
    /// </summary>
    internal sealed class ExtendedPropertyAddOrUpdatePositionalTransform : ISqlBlockTransform
    {
        private static readonly Regex R = new Regex(
            // EXEC/EXECUTE + 可选 sys. 前缀；兼容位置参数或带 @name= 的写法，但本规则主要面向位置参数
            @"^\s*EXEC(?:UTE)?\s+(?:sys\.)?sp_addextendedproperty\s+" +
            // @name or positional #1
            @"(?:(?:@name\s*=\s*)?N?'(?<pname>[^']+)')\s*,\s*" +
            // @value or positional #2
            @"(?:(?:@value\s*=\s*)?N?'(?<pvalue>[^']*)')\s*,\s*" +
            // level0type: SCHEMA
            @"(?:(?:@level0type\s*=\s*)?N?'SCHEMA')\s*,\s*" +
            // level0name: schema
            @"(?:(?:@level0name\s*=\s*)?N?'(?<schema>[^']+)')\s*,\s*" +
            // level1type: TABLE
            @"(?:(?:@level1type\s*=\s*)?N?'TABLE')\s*,\s*" +
            // level1name: table
            @"(?:(?:@level1name\s*=\s*)?N?'(?<table>[^']+)')\s*,\s*" +
            // level2type: COLUMN
            @"(?:(?:@level2type\s*=\s*)?N?'COLUMN')\s*,\s*" +
            // level2name: column
            @"(?:(?:@level2name\s*=\s*)?N?'(?<column>[^']+)')\s*;?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant
        );

        public bool CanHandle(string block) => R.IsMatch(block);

        public string Transform(string block)
        {
            var m = R.Match(block);
            var pname = m.Groups["pname"].Value;
            var schema = m.Groups["schema"].Value;
            var table = m.Groups["table"].Value;
            var column = m.Groups["column"].Value;

            // 用原始语句分别生成 add / update 版本，确保参数顺序与风格（位置参数/多行）保持一致
            var addStmt = Regex.Replace(block, @"\bsp_updateextendedproperty\b", "sp_addextendedproperty", RegexOptions.IgnoreCase);
            var updateStmt = Regex.Replace(block, @"\bsp_addextendedproperty\b", "sp_updateextendedproperty", RegexOptions.IgnoreCase);

            // 列级扩展属性存在性判断：class=1(对象), major_id=表, minor_id=列
            var existsExpr =
                $"ep.class = 1 AND ep.major_id = OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}') " +
                $"AND ep.minor_id = COLUMNPROPERTY(OBJECT_ID(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}'), N'{SqlId.Unquote(column)}', 'ColumnId')";

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
