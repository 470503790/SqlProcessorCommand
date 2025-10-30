# SqlProcessorCommand

> 将原始 SQL Server 脚本转换为“幂等”脚本：可多次重复执行而不会因为对象已存在/不存在等原因失败，降低发布脚本的风险。

## 背景
常规升级脚本里包含 `CREATE TABLE / CREATE PROC / DROP COLUMN / DROP TABLE / ALTER COLUMN` 等语句，若直接重复执行容易报错或造成危险行为。本工具按 `GO` 分批读取输入脚本，针对每一批尝试匹配一条规则（Transform），将其改写为“存在才创建/删除”或“条件包装”，或直接丢弃高风险语句，使整体脚本具备幂等能力。

## 使用场景

### 典型应用场景
1. **数据库版本升级**: 将开发环境的升级脚本转换为可重复执行的生产脚本
2. **多环境部署**: 确保同一脚本可以在测试、预生产、生产环境重复执行
3. **CI/CD 管道**: 自动化部署流程中的数据库变更管理
4. **应急修复**: 紧急补丁的安全部署，避免因对象已存在而失败
5. **数据库迁移**: 跨服务器的数据库结构同步

### 推荐工作流程
```bash
# 1. 从原始升级脚本生成幂等版本
SqlProcessorCommand -i upgrade_v1.2.sql -n safe

# 2. 审阅生成的脚本确保符合预期
notepad upgrade_v1.2.safe.sql

# 3. 在测试环境验证
sqlcmd -S test-server -d MyDatabase -i upgrade_v1.2.safe.sql

# 4. 生产环境部署（可重复执行）
sqlcmd -S prod-server -d MyDatabase -i upgrade_v1.2.safe.sql
```

## 命令行使用
```
SqlProcessorCommand --input <input.sql> [--output <out.sql>] [--name <tag>] [--encoding <enc>]
                    [--discard-drop-table | --keep-drop-table]
                    [--discard-drop-column | --keep-drop-column]
                    [--discard-drop-constraint | --keep-drop-constraint]
                    [--discard-drop-index | --keep-drop-index]
```

### 参数说明
| 选项 | 说明 | 默认 |
|------|------|------|
| `-i, --input` | 输入 SQL 文件（必填） | - |
| `-o, --output` | 输出文件路径；若省略：`<输入名>.<tag|idempotent>.sql` | 自动生成 |
| `-n, --name` | 输出文件名中使用的自定义后缀（如 `upgrade`） | `idempotent` |
| `--encoding` | 文件编码（读写一致），示例：`utf-8`, `gbk`, `utf-16` | `utf-8` |
| `--discard-drop-table` | 丢弃所有 `DROP TABLE` 语句 | 开启 |
| `--keep-drop-table` | 保留 `DROP TABLE`，但会包装成“存在才删” | - |
| `--discard-drop-column` | 丢弃所有 `ALTER TABLE ... DROP COLUMN` | 开启 |
| `--keep-drop-column` | 保留列删除，包装成“存在才删” | - |
| `--discard-drop-constraint` | 丢弃所有 `ALTER TABLE ... DROP CONSTRAINT` | 开启 |
| `--keep-drop-constraint` | 保留约束删除，包装成"存在才删" | - |
| `--discard-drop-index` | 丢弃所有 `DROP INDEX` 语句 | 开启 |
| `--keep-drop-index` | 保留索引删除，包装成"存在才删" | - |
| `-h, --help` | 显示帮助 | - |

> `--discard-*` 与 `--keep-*` 二选一；若都不写，默认“丢弃”。

### 最简单示例
```
SqlProcessorCommand -i upgrade.sql
```
输出：`upgrade.idempotent.sql`

### 自定义后缀与保留 DROP
```
SqlProcessorCommand -i upgrade.sql -n safe --keep-drop-table --keep-drop-column --keep-drop-constraint --keep-drop-index
```
```
输出：`upgrade.safe.sql`

## 转换示例
### 1. CREATE PROCEDURE / VIEW / FUNCTION / TRIGGER
输入：
```sql
CREATE PROCEDURE dbo.p_demo AS SELECT 1;
GO
```
输出（根据版本包装为 CREATE OR ALTER，如目标 2014 采用兼容包裹）：
```sql
IF OBJECT_ID(N'dbo.p_demo', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.p_demo AS SELECT 1;');
GO
ALTER PROCEDURE dbo.p_demo AS SELECT 1;
GO
```

### 2. DROP TABLE（默认丢弃）
输入：
```sql
DROP TABLE [dbo].[T_A];
```
默认输出：空（语句被移除）。

若使用 `--keep-drop-table`：
```sql
IF OBJECT_ID(N'[dbo].[T_A]', N'U') IS NOT NULL
    DROP TABLE [dbo].[T_A];
```

### 3. ALTER TABLE ... DROP COLUMN（默认丢弃）
输入：
```sql
ALTER TABLE dbo.T_A DROP COLUMN ObsoleteFlag;
```
默认输出：空。

使用 `--keep-drop-column` 输出：
```sql
IF COL_LENGTH(N'[dbo].[T_A]', N'ObsoleteFlag') IS NOT NULL
BEGIN
    ALTER TABLE dbo.T_A DROP COLUMN ObsoleteFlag;
END
```

### 4. DROP CONSTRAINT（默认丢弃）
输入：
```sql
ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint];
```
默认输出：空（语句被移除）。

若使用 `--keep-drop-constraint`：
```sql
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_TestConstraint' AND parent_object_id = OBJECT_ID(N'[dbo].[TestTable]'))
BEGIN
    ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint];
END
```

**注意：默认约束（Default Constraint）的特殊处理**

默认约束（通常名称以 `DF_` 或 `DF__` 开头）不会被上述 DROP CONSTRAINT 规则处理，而是由专门的 `DropDefaultConstraintSmartTransform` 转换器处理。这是因为：

1. **约束名可能未知**：开发人员通常不知道 SQL Server 自动生成的默认约束名称（如 `DF__table__col__12345678`）
2. **智能删除**：该转换器支持通过列名删除默认约束，语法为：`ALTER TABLE ... DROP DEFAULT FOR [column]`
3. **自动查询**：转换器会生成代码，从系统表（`sys.default_constraints`）中查询绑定到指定列的默认约束名，然后执行删除

示例 - 智能删除默认约束：
输入：
```sql
ALTER TABLE [dbo].[Orders] DROP DEFAULT FOR [Status];
```
输出：
```sql
DECLARE @dcname sysname;
SELECT @dcname = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Orders]')
  AND c.name = N'Status';

IF @dcname IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [' + @dcname + ']');
END
```

如果你明确知道默认约束的名称（如 `DF_Orders_Status`），也可以直接使用 `DROP CONSTRAINT` 语法，但该语句会被识别为默认约束并交由智能处理器处理。

### 5. DROP INDEX（默认丢弃）
输入：
```sql
DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable];
```
默认输出：空（语句被移除）。

若使用 `--keep-drop-index`：
```sql
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TestTable_Column' AND object_id = OBJECT_ID(N'[dbo].[TestTable]'))
BEGIN
    DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable];
END
```

### 6. ALTER COLUMN
输入：
```sql
ALTER TABLE dbo.T_A ALTER COLUMN Name NVARCHAR(100) NULL;
```
输出：
```sql
IF COL_LENGTH(N'[dbo].[T_A]', N'Name') IS NOT NULL
BEGIN
    ALTER TABLE dbo.T_A ALTER COLUMN Name NVARCHAR(100) NULL;
END
```

### 7. ADD COLUMN / ADD CONSTRAINT / 默认约束
类似地被改写为“列/约束不存在时才添加”。
### 8. CREATE SCHEMA
输入：
```sql
CREATE SCHEMA [TestSchema]
```
输出：
```sql
IF SCHEMA_ID(N'[TestSchema]') IS NULL
BEGIN
    CREATE SCHEMA [TestSchema]
END
```

### 9. CREATE USER-DEFINED TYPE
输入：
```sql
CREATE TYPE [dbo].[MyTableType] AS TABLE (
    ID INT NOT NULL,
    Name NVARCHAR(50)
)
```
输出：
```sql
IF TYPE_ID(N'[dbo].[MyTableType]') IS NULL
BEGIN
    CREATE TYPE [dbo].[MyTableType] AS TABLE (
        ID INT NOT NULL,
        Name NVARCHAR(50)
    )
END
```

### 10. CREATE SYNONYM
输入：
```sql
CREATE SYNONYM [dbo].[MyTableSyn] FOR [dbo].[MyTable]
```
输出：
```sql
IF OBJECT_ID(N'[dbo].[MyTableSyn]', N'SN') IS NULL
BEGIN
    CREATE SYNONYM [dbo].[MyTableSyn] FOR [dbo].[MyTable]
END
```

### 11. CREATE SEQUENCE (SQL Server 2012+)
输入：
```sql
CREATE SEQUENCE [dbo].[MySequence] START WITH 1 INCREMENT BY 1
```
输出：
```sql
IF OBJECT_ID(N'[dbo].[MySequence]', N'SO') IS NULL
BEGIN
    CREATE SEQUENCE [dbo].[MySequence] START WITH 1 INCREMENT BY 1
END
```

### 12. CREATE ROLE
输入：
```sql
CREATE ROLE [TestRole]
```
输出：
```sql
IF DATABASE_PRINCIPAL_ID(N'[TestRole]') IS NULL
BEGIN
    CREATE ROLE [TestRole]
END
```

## SQL Server 2014 兼容性说明

本工具专门针对 SQL Server 2014 常用的升级脚本操作进行优化，涵盖了以下主要场景：

### 数据定义语言 (DDL) 支持
- **表操作**: CREATE TABLE, DROP TABLE, ALTER TABLE
- **列操作**: ADD COLUMN, ALTER COLUMN, DROP COLUMN  
- **索引操作**: CREATE INDEX, DROP INDEX（支持 UNIQUE/CLUSTERED/NONCLUSTERED）
- **约束操作**: ADD CONSTRAINT, DROP CONSTRAINT, 默认约束管理
- **架构管理**: CREATE SCHEMA, DROP SCHEMA
- **存储过程**: CREATE PROCEDURE, ALTER PROCEDURE, CREATE OR ALTER（2014 兼容包装）
- **视图**: CREATE VIEW, ALTER VIEW, CREATE OR ALTER（2014 兼容包装）
- **函数**: CREATE FUNCTION, ALTER FUNCTION（2014 兼容包装）
- **触发器**: CREATE TRIGGER, ALTER TRIGGER（2014 兼容包装）
- **用户定义类型**: CREATE TYPE (标量/表类型), DROP TYPE
- **同义词**: CREATE SYNONYM, DROP SYNONYM
- **序列**: CREATE SEQUENCE, ALTER SEQUENCE, DROP SEQUENCE（SQL 2012+ 功能）

### 安全对象支持  
- **角色管理**: CREATE ROLE, DROP ROLE
- **用户管理**: CREATE USER, DROP USER

### 扩展属性支持
- **表级扩展属性**: sp_addextendedproperty / sp_updateextendedproperty（自动条件化）
- **列级扩展属性**: 完整的位置参数与命名参数支持

### 2014 特有功能
- **CREATE OR ALTER 包装**: 自动转换为 2014 兼容的 DROP + CREATE 模式
- **安全检查**: 所有 DROP 操作都通过系统视图进行存在性验证
- **批处理支持**: 正确处理 GO 分隔的批处理脚本
按执行顺序（概念分组）：
- 危险删除类（可丢弃或条件包装）
  - `DropTableToEmptyTransform` / `DropTableTransform`
  - `DropColumnToEmptyTransform` / `DropColumnTransform`
- 结构与列变更
  - `AddDefaultConstraintTransform`
  - `AddConstraintTransform`
  - `AddColumnTransform`
  - `CreateTableTransform`
  - `AlterColumnTransform`
- 扩展属性
  - `ExtendedPropertyAddOrUpdateTablePositionalTransform`
  - `ExtendedPropertyAddOrUpdatePositionalTransform`
- 对象创建包装（跨版本兼容）
  - `CreateOrAlterProcedureTransform`
  - `CreateOrAlterViewTransform`
  - `CreateOrAlterTriggerTransform`
  - `ProcCreateAlterWrapper2014`
  - `ViewCreateAlterWrapper2014`
  - `TriggerCreateAlterWrapper2014`
  - `FunctionCreateAlterWrapper2014`
- 索引 / 约束 / 其他
  - `CreateIndexTransform`
  - `DropConstraintTransform`
  - `DropDefaultConstraintSmartTransform`
  - `DropIndexTransform`
- 架构管理
  - `CreateSchemaTransform`
  - `DropSchemaTransform`
- 用户定义类型
  - `CreateUserDefinedTypeTransform`
  - `DropUserDefinedTypeTransform`
- 同义词
  - `CreateSynonymTransform`
  - `DropSynonymTransform`
- 序列对象（SQL Server 2012+）
  - `CreateSequenceTransform`
  - `AlterSequenceTransform`
  - `DropSequenceTransform`
- 安全对象
  - `CreateRoleTransform`
  - `DropRoleTransform`
  - `CreateUserTransform`
  - `DropUserTransform`
- 清理
  - `ColumnPrefixDiscardTransform`
  - （以及可扩展的 `GenericRegexWrapTransform` 等自定义包装）

每个规则实现接口：
```
public interface ISqlBlockTransform {
    bool CanHandle(string block);
    string Transform(string block);
}
```
命中后即停止对该批次继续匹配（单条批次只应用一个 Transform）。

## 作为库调用
```csharp
using SqlProcessorCommand;

var options = new SqlIdempotentProcessor.Options {
    DiscardDropTable = true,          // 或 false，默认 true
    DiscardDropColumn = true,         // 或 false，默认 true
    DiscardDropConstraint = true,     // 或 false，默认 true
    DiscardDropIndex = true           // 或 false，默认 true
};
var proc = new SqlIdempotentProcessor(options);
string outputSql = proc.Transform(File.ReadAllText("upgrade.sql"));
File.WriteAllText("upgrade.idempotent.sql", outputSql);
```

## 自定义管道
可参考 `SqlIdempotentProcessor.BuildDefaultPipeline` 了解顺序和策略：
1. Fork 项目后新增自定义 `ISqlBlockTransform` 实现。
2. 在构建管道处插入 `p.Add(new YourTransform());`。
3. 重新编译使用。

若需更精细的多规则匹配，可修改遍历逻辑（默认命中即停止）。

## GO 分批逻辑
- 仅识别“独立行的 GO”（大小写不敏感），允许后缀 `-- 注释`。
- 行尾与下一批之间重新插入 `GO` 分隔，保证输出脚本结构清晰。

## 编译
- 目标框架：.NET Framework 4.8
- 直接用 Visual Studio 或 `msbuild` / `dotnet build` (需安装 net48 targeting pack) 编译。

## 注意事项 / 限制
- 当前未深度解析列类型变更，只在列存在时执行 `ALTER COLUMN`。
- 不处理跨批次依赖的重排（假设原始顺序正确）。
- 只处理常见语法形态；非常规写法可能不会命中规则。
- 请在生产前审阅生成的幂等脚本，确保符合业务预期。
- 权限相关语句（GRANT/REVOKE/DENY）暂未支持自动包装。
- CREATE LOGIN 等服务器级对象暂未纳入处理范围。

## 支持的完整 SQL 操作清单

### ✅ 已支持的操作
| 操作类型 | SQL 语句 | 检查条件 | 备注 |
|---------|----------|----------|------|
| 表管理 | `CREATE TABLE` | `OBJECT_ID(..., 'U')` | 用户表 |
| | `DROP TABLE` | `OBJECT_ID(..., 'U')` | 可配置丢弃或包装 |
| 列管理 | `ADD COLUMN` | `COL_LENGTH(...)` | 添加新列 |
| | `ALTER COLUMN` | `COL_LENGTH(...)` | 修改列定义 |
| | `DROP COLUMN` | `COL_LENGTH(...)` | 可配置丢弃或包装 |
| 索引管理 | `CREATE INDEX` | `sys.indexes` | 所有索引类型 |
| | `DROP INDEX` | `sys.indexes` | 安全删除 |
| 约束管理 | `ADD CONSTRAINT` | `sys.objects` | 各类约束 |
| | `DROP CONSTRAINT` | `sys.objects` | 安全删除 |
| | 默认约束 | 智能检测 | DF_ 约束管理 |
| 存储过程 | `CREATE PROCEDURE` | `OBJECT_ID(..., 'P')` | 2014 兼容包装 |
| | `CREATE OR ALTER PROCEDURE` | 转换处理 | 自动转换语法 |
| 视图 | `CREATE VIEW` | `OBJECT_ID(..., 'V')` | 2014 兼容包装 |
| | `CREATE OR ALTER VIEW` | 转换处理 | 自动转换语法 |
| 函数 | `CREATE FUNCTION` | `OBJECT_ID(...)` | 2014 兼容包装 |
| | `CREATE OR ALTER FUNCTION` | 转换处理 | 自动转换语法 |
| 触发器 | `CREATE TRIGGER` | `OBJECT_ID(..., 'TR')` | 2014 兼容包装 |
| | `CREATE OR ALTER TRIGGER` | 转换处理 | 自动转换语法 |
| 架构 | `CREATE SCHEMA` | `SCHEMA_ID(...)` | 架构管理 |
| | `DROP SCHEMA` | `SCHEMA_ID(...)` | 安全删除 |
| 用户类型 | `CREATE TYPE` | `TYPE_ID(...)` | 标量/表类型 |
| | `DROP TYPE` | `TYPE_ID(...)` | 安全删除 |
| 同义词 | `CREATE SYNONYM` | `OBJECT_ID(..., 'SN')` | 同义词管理 |
| | `DROP SYNONYM` | `OBJECT_ID(..., 'SN')` | 安全删除 |
| 序列 | `CREATE SEQUENCE` | `OBJECT_ID(..., 'SO')` | SQL 2012+ |
| | `ALTER SEQUENCE` | `OBJECT_ID(..., 'SO')` | 参数修改 |
| | `DROP SEQUENCE` | `OBJECT_ID(..., 'SO')` | 安全删除 |
| 安全 | `CREATE ROLE` | `DATABASE_PRINCIPAL_ID(...)` | 数据库角色 |
| | `DROP ROLE` | `DATABASE_PRINCIPAL_ID(...)` | 安全删除 |
| | `CREATE USER` | `DATABASE_PRINCIPAL_ID(...)` | 数据库用户 |
| | `DROP USER` | `DATABASE_PRINCIPAL_ID(...)` | 安全删除 |
| 扩展属性 | `sp_addextendedproperty` | `sys.extended_properties` | 表级/列级 |
| | `sp_updateextendedproperty` | 自动转换 | 存在则更新 |

### ❌ 暂不支持的操作
- `GRANT` / `REVOKE` / `DENY` 权限管理语句
- `CREATE LOGIN` / `ALTER LOGIN` / `DROP LOGIN` 服务器级登录
- `CREATE DATABASE` / `ALTER DATABASE` 数据库级操作
- `CREATE PARTITION FUNCTION/SCHEME` 分区对象
- `CREATE AGGREGATE` 用户定义聚合函数
- `CREATE ASSEMBLY` .NET 程序集（SQL CLR）
- `CREATE CERTIFICATE` / `CREATE KEY` 加密对象

## License
本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

---
欢迎提交 Issue / PR 以补充更多幂等规则或改进正则匹配。
