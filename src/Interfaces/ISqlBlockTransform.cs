namespace SqlProcessorCommand
{
    public interface ISqlBlockTransform
    {
        bool CanHandle(string block);
        string Transform(string block);
    }
}
