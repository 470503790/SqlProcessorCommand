namespace SqlProcessorCommand
{
    /// <summary>
    /// 通用占位规则（示例/基类思想展示）。此实现不处理任何语句，仅作文档保留。
    /// </summary>
    internal sealed class GenericRegexWrapTransform : ISqlBlockTransform
    {
        public bool CanHandle(string block) => false;
        public string Transform(string block) => block;
    }
}
