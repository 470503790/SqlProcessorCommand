using SqlProcessorCommand.src.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlProcessorCommand
{
    /// <summary>
    /// 幂等性 SQL 处理器：负责将输入脚本按独立行 <c>GO</c> 分批，
    /// 逐批经过 Transforms 管道处理，并合并输出。
    /// </summary>
    public class SqlIdempotentProcessor
    {
        public class Options
        {
            /// <summary>
            /// 丢弃 DROP TABLE 语句（默认 true）。
            /// </summary>
            public bool DiscardDropTable { get; set; } = true;
            /// <summary>
            /// 丢弃 ALTER TABLE ... DROP COLUMN 语句（默认 true）。
            /// </summary>
            public bool DiscardDropColumn { get; set; } = true;
            /// <summary>
            /// 丢弃 ALTER TABLE ... DROP CONSTRAINT 语句（默认 true）。
            /// </summary>
            public bool DiscardDropConstraint { get; set; } = true;
            /// <summary>
            /// 丢弃 DROP INDEX 语句（默认 true）。
            /// </summary>
            public bool DiscardDropIndex { get; set; } = true;
        }

        private readonly List<ISqlBlockTransform> _pipeline;

        public SqlIdempotentProcessor(Options options)
        {
            _options = options ?? new Options();
            _pipeline = BuildDefaultPipeline(_options);
        }

        public Options _options { get; }

        /// <summary>
        /// 总入口：对整个 SQL 文本做幂等转换。
        /// </summary>
        public string Transform(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var blocks = SplitBatchesByGo(text);
            var outBlocks = new List<string>(blocks.Count);

            foreach (var b in blocks)
            {
                var t = b;
                foreach (var x in _pipeline)
                {
                    if (x.CanHandle(t))
                    {
                        t = x.Transform(t);
                        // 单条命中一个规则后直接结束（单一职责，避免重复包裹）
                        break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(t))
                    outBlocks.Add(t.TrimEnd());
            }

            // 在批次之间插回 GO
            return string.Join(Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine, outBlocks) + Environment.NewLine;
        }

        private static List<string> SplitBatchesByGo(string text)
        {
            // 仅在“独立行的 GO”处分割；大小写不敏感；允许行尾注释
            var lines = Regex.Split(text.Replace("\r\n", "\n"), "\n");
            var batches = new List<string>();
            var sb = new StringBuilder();
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (Regex.IsMatch(line, @"^(?i:GO)(?:\s+--.*)?$"))
                {
                    var batch = sb.ToString().TrimEnd();
                    if (!string.IsNullOrWhiteSpace(batch))
                        batches.Add(batch);
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(raw);
                }
            }
            var last = sb.ToString().TrimEnd();
            if (!string.IsNullOrWhiteSpace(last))
                batches.Add(last);
            return batches;
        }

        private static List<ISqlBlockTransform> BuildDefaultPipeline(Options opt)
        {
            var p = new PipelineBuilder();

            // 注意顺序：先危险/结构性再元数据
            if (opt.DiscardDropTable)
            {
                p.Add(new DropTableToEmptyTransform());           // 丢弃 DROP TABLE
            }
            else
            {
                p.Add(new DropTableTransform());                  // DROP TABLE 安全化/移除
            }

            if (opt.DiscardDropColumn)
            {
                p.Add(new DropColumnToEmptyTransform());       // 丢弃 DROP COLUMN
            }
            else
            {
                p.Add(new DropColumnTransform());              // 条件 DROP COLUMN
            }

            p.Add(new AddDefaultConstraintTransform());        // DF_ 默认约束
            p.Add(new AddConstraintTransform());               // 其他显式约束（存在则跳过）
            p.Add(new AddColumnTransform());                   // ADD COLUMN
            p.Add(new CreateTableTransform());                 // CREATE TABLE

            p.Add(new ExtendedPropertyAddOrUpdateTablePositionalTransform());    // 扩展属性（表级）
            p.Add(new ExtendedPropertyAddOrUpdatePositionalTransform());            // 扩展属性（列级）

            // 对象创建包装（2014 兼容）
            p.Add(new CreateOrAlterProcedureTransform());
            p.Add(new CreateOrAlterViewTransform());
            p.Add(new CreateOrAlterTriggerTransform());
            p.Add(new CreateOrAlterFunctionTransform());
            p.Add(new ProcCreateAlterWrapper2014());
            p.Add(new ViewCreateAlterWrapper2014());
            p.Add(new TriggerCreateAlterWrapper2014());
            p.Add(new FunctionCreateAlterWrapper2014());

            // 索引/约束/杂项
            p.Add(new CreateIndexTransform());
            
            if (opt.DiscardDropConstraint)
            {
                p.Add(new DropConstraintToEmptyTransform());       // 丢弃 DROP CONSTRAINT
            }
            else
            {
                p.Add(new DropConstraintTransform());              // DROP CONSTRAINT 安全化
            }
            
            p.Add(new DropDefaultConstraintSmartTransform());
            
            if (opt.DiscardDropIndex)
            {
                p.Add(new DropIndexToEmptyTransform());            // 丢弃 DROP INDEX
            }
            else
            {
                p.Add(new DropIndexTransform());                   // DROP INDEX 安全化
            }

            // 架构管理
            p.Add(new CreateSchemaTransform());
            p.Add(new DropSchemaTransform());

            // 用户定义类型
            p.Add(new CreateUserDefinedTypeTransform());
            p.Add(new DropUserDefinedTypeTransform());

            // 同义词
            p.Add(new CreateSynonymTransform());
            p.Add(new DropSynonymTransform());

            // 序列（SQL 2012+）
            p.Add(new CreateSequenceTransform());
            p.Add(new AlterSequenceTransform());
            p.Add(new DropSequenceTransform());

            // 安全对象
            p.Add(new CreateRoleTransform());
            p.Add(new DropRoleTransform());
            p.Add(new CreateUserTransform());
            p.Add(new DropUserTransform());

            p.Add(new AlterColumnTransform());                 // 最后处理 ALTER COLUMN 类

            // 清理项
            p.Add(new ColumnPrefixDiscardTransform());         // 可选前缀清理（如果命中）

            return p.Build();
        }
    }
}
