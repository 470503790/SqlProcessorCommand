using System;
using System.IO;
using System.Text;

namespace SqlProcessorCommand
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || HasFlag(args, "-h") || HasFlag(args, "--help"))
                {
                    PrintHelp();
                    return 0;
                }

                string input = GetOption(args, "--input") ?? GetOption(args, "-i");
                string output = GetOption(args, "--output") ?? GetOption(args, "-o");
                string tag = GetOption(args, "--name") ?? GetOption(args, "-n");
                string encName = GetOption(args, "--encoding");

                bool discardAllDrops = HasFlag(args, "--discard-all-drops");

                bool? discardDropTable = null; // null=默认(true); true=丢弃; false=保留
                if (HasFlag(args, "--discard-drop-table")) discardDropTable = true;
                if (HasFlag(args, "--keep-drop-table"))    discardDropTable = false;

                bool? discardDropColumn = null; // null=默认(true); true=丢弃; false=保留
                if (HasFlag(args, "--discard-drop-column")) discardDropColumn = true;
                if (HasFlag(args, "--keep-drop-column"))    discardDropColumn = false;

                bool? discardDropConstraint = null; // null=默认(true); true=丢弃; false=保留
                if (HasFlag(args, "--discard-drop-constraint")) discardDropConstraint = true;
                if (HasFlag(args, "--keep-drop-constraint"))    discardDropConstraint = false;

                bool? discardDropIndex = null; // null=默认(true); true=丢弃; false=保留
                if (HasFlag(args, "--discard-drop-index")) discardDropIndex = true;
                if (HasFlag(args, "--keep-drop-index"))    discardDropIndex = false;

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.Error.WriteLine("ERROR: 必须指定输入文件，使用 -i <path> 或 --input <path>。");
                    PrintHelp();
                    return 2;
                }
                if (!File.Exists(input))
                {
                    Console.Error.WriteLine("ERROR: 找不到输入文件: " + input);
                    return 3;
                }

                if (string.IsNullOrWhiteSpace(output))
                {
                    var fi = new FileInfo(input);
                    var suffix = string.IsNullOrWhiteSpace(tag) ? "idempotent" : tag.Trim();
                    output = Path.Combine(fi.DirectoryName ?? ".", Path.GetFileNameWithoutExtension(fi.Name) + "." + suffix + ".sql");
                }

                Encoding enc = Encoding.UTF8;
                if (!string.IsNullOrWhiteSpace(encName))
                {
                    enc = Encoding.GetEncoding(encName);
                }

                var text = File.ReadAllText(input, enc);

                var options = new SqlIdempotentProcessor.Options
                {
                    DiscardAllDrops = discardAllDrops,
                    DiscardDropTable = discardDropTable ?? true,
                    DiscardDropColumn = discardDropColumn ?? true,
                    DiscardDropConstraint = discardDropConstraint ?? true,
                    DiscardDropIndex = discardDropIndex ?? true,
                };

                var processor = new SqlIdempotentProcessor(options);
                var outText = processor.Transform(text);

                File.WriteAllText(output, outText, enc);
                Console.WriteLine("已生成幂等 SQL: " + Path.GetFullPath(output));
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FATAL: " + ex.Message);
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static string GetOption(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length) return args[i + 1];
                    return null;
                }
                // 支持 --name=value 形式
                if (args[i].StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
                {
                    return args[i].Substring(name.Length + 1);
                }
            }
            return null;
        }

        private static bool HasFlag(string[] args, string name)
        {
            foreach (var a in args)
            {
                if (string.Equals(a, name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("用法:");
            Console.WriteLine("  SqlProcessorCommand --input <input.sql> [--output <out.sql>] [--name <tag>] [--encoding <enc>]");
            Console.WriteLine("                       [--discard-all-drops]");
            Console.WriteLine("                       [--discard-drop-table | --keep-drop-table]");
            Console.WriteLine("                       [--discard-drop-column | --keep-drop-column]");
            Console.WriteLine("                       [--discard-drop-constraint | --keep-drop-constraint]");
            Console.WriteLine("                       [--discard-drop-index | --keep-drop-index]");
            Console.WriteLine();
            Console.WriteLine("参数:");
            Console.WriteLine("  -i, --input            输入 SQL 文件路径（必填）");
            Console.WriteLine("  -o, --output           输出 SQL 文件路径（可选；默认: <输入名>.<name|idempotent>.sql）");
            Console.WriteLine("  -n, --name             命名（用于输出文件名后缀；可选）");
            Console.WriteLine("      --encoding         文本编码，默认 utf-8（示例：gb2312, gbk, utf-16）");
            Console.WriteLine("      --discard-all-drops        丢弃所有 DROP 语句（优先级最高，覆盖所有其他 DROP 选项）");
            Console.WriteLine("      --discard-drop-table       丢弃所有 DROP TABLE 语句（默认开启）");
            Console.WriteLine("      --keep-drop-table          保留 DROP TABLE 语句（覆盖默认）");
            Console.WriteLine("      --discard-drop-column      丢弃所有 ALTER TABLE ... DROP COLUMN 语句（默认开启）");
            Console.WriteLine("      --keep-drop-column         保留 DROP COLUMN 语句（覆盖默认）");
            Console.WriteLine("      --discard-drop-constraint  丢弃所有 ALTER TABLE ... DROP CONSTRAINT 语句（默认开启）");
            Console.WriteLine("      --keep-drop-constraint     保留 DROP CONSTRAINT 语句（覆盖默认）");
            Console.WriteLine("      --discard-drop-index       丢弃所有 DROP INDEX 语句（默认开启）");
            Console.WriteLine("      --keep-drop-index          保留 DROP INDEX 语句（覆盖默认）");
            Console.WriteLine("  -h, --help             显示帮助");
        }
    }
}
