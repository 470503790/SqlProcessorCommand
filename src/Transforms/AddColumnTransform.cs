using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 处理语句：<c>ALTER TABLE [schema].[table] ADD [column] ...</c><br/>
    /// - 通过 <c>COL_LENGTH('[schema].[table]','column')</c> 判断列是否已存在；<br/>
    /// - 仅当列不存在时才执行 ADD；<br/>
    /// - 适配方括号/无括号标识符，大小写不敏感。
    /// </summary>
    internal sealed class AddColumnTransform : ISqlBlockTransform
    {
        private static readonly Regex R =
            // 说明：原先列名后使用 \b，遇到方括号列名（如 [plan_export_qty]）后紧跟空格时不成立，改为正向预查匹配空白/括号/行尾
            new Regex(@"^\s*ALTER\s+TABLE\s+(?:(?:\[(?<schema>[^\]]+)\]|(?<schema2>\w+))\.)?(?:\[(?<table>[^\]]+)\]|(?<table2>\w+))\s+ADD\s+(?:\[(?<col>[^\]]+)\]|(?<col2>\w+))(?=\s|\(|$)(?<rest>[\s\S]*)$",
                      RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        public bool CanHandle(string block) => R.IsMatch(block);
        public string Transform(string block)
        {
            var m = R.Match(block);
            var schema = m.Groups["schema"].Success ? m.Groups["schema"].Value : 
                         m.Groups["schema2"].Success ? m.Groups["schema2"].Value : "dbo";
            var table = m.Groups["table"].Success ? m.Groups["table"].Value : m.Groups["table2"].Value;
            var col = m.Groups["col"].Success ? m.Groups["col"].Value : m.Groups["col2"].Value;
            return $@"
IF COL_LENGTH(N'{SqlId.Quote(schema)}.{SqlId.Quote(table)}', N'{SqlId.Unquote(col)}') IS NULL
BEGIN
    {block}
END".Trim();
        }
    }
}
