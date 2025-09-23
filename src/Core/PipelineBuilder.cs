using System.Collections.Generic;
using System.Linq;

namespace SqlProcessorCommand
{
    public class PipelineBuilder
    {
        private readonly List<ISqlBlockTransform> _list = new List<ISqlBlockTransform>();
        public PipelineBuilder Add(ISqlBlockTransform t) { _list.Add(t); return this; }
        public System.Collections.Generic.List<ISqlBlockTransform> Build() => _list.ToList();
    }
}
