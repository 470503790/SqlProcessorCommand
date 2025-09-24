# SqlProcessorCommand v1.0.0 Release Notes

## 🎉 首次发布！

SqlProcessorCommand 是一个专为 SQL Server 数据库升级脚本设计的幂等性转换工具。它能将常规的数据库升级脚本转换为可以安全重复执行的脚本，大大降低生产环境数据库部署的风险。

## ✨ 主要特性

### 核心功能
- **幂等性转换**: 自动将 SQL 脚本转换为可重复执行的安全版本
- **智能检测**: 自动检测对象是否存在，避免重复创建或删除不存在的对象
- **灵活配置**: 支持配置是否保留或丢弃危险的 DROP 操作
- **批处理支持**: 按 GO 语句分批处理，保持脚本结构清晰

### 支持的 SQL 操作

#### ✅ 表和列管理
- `CREATE TABLE` - 检查表是否存在
- `DROP TABLE` - 可配置丢弃或条件包装
- `ADD COLUMN` - 检查列是否存在
- `ALTER COLUMN` - 条件修改列定义
- `DROP COLUMN` - 可配置丢弃或条件包装

#### ✅ 索引和约束
- `CREATE INDEX` - 检查索引是否存在
- `DROP INDEX` - 安全删除索引
- `ADD CONSTRAINT` - 各类约束的条件添加
- `DROP CONSTRAINT` - 安全删除约束

#### ✅ 数据库对象 
- `CREATE/ALTER PROCEDURE` - 存储过程（支持 2014 兼容模式）
- `CREATE/ALTER VIEW` - 视图（支持 2014 兼容模式）
- `CREATE/ALTER TRIGGER` - 触发器
- `CREATE/ALTER FUNCTION` - 各类函数
- `CREATE/DROP SEQUENCE` - 序列对象（SQL 2012+）
- `CREATE/DROP TYPE` - 用户定义类型
- `CREATE SCHEMA` - 数据库架构

#### ✅ 安全和元数据
- `CREATE/DROP ROLE` - 数据库角色
- `CREATE/DROP USER` - 数据库用户  
- `sp_addextendedproperty` - 扩展属性（自动转换为更新模式）

## 🚀 使用方法

### 命令行使用
```bash
# 最简单的用法
SqlProcessorCommand -i upgrade.sql

# 自定义输出文件后缀
SqlProcessorCommand -i upgrade.sql -n safe

# 保留 DROP 语句但包装为安全执行
SqlProcessorCommand -i upgrade.sql --keep-drop-table --keep-drop-column

# 指定文件编码
SqlProcessorCommand -i upgrade.sql --encoding gbk
```

### 作为库使用
```csharp
using SqlProcessorCommand;

var options = new SqlIdempotentProcessor.Options {
    DiscardDropTable = true,      // 丢弃 DROP TABLE
    DiscardDropColumn = true      // 丢弃 DROP COLUMN  
};

var processor = new SqlIdempotentProcessor(options);
string outputSql = processor.Transform(File.ReadAllText("upgrade.sql"));
File.WriteAllText("upgrade.idempotent.sql", outputSql);
```

## 📖 典型使用场景

1. **数据库版本升级**: 将开发环境的升级脚本转换为生产可用脚本
2. **多环境部署**: 确保脚本在测试、预生产、生产环境都能重复执行
3. **CI/CD 管道**: 数据库变更的自动化部署
4. **应急修复**: 紧急补丁的安全部署
5. **数据库迁移**: 跨服务器的数据库结构同步

## 💡 转换示例

**输入脚本:**
```sql
CREATE TABLE Users (
    Id int PRIMARY KEY,
    Name nvarchar(100)
);
GO

CREATE PROCEDURE GetUser(@Id int) 
AS
SELECT * FROM Users WHERE Id = @Id;
GO
```

**输出脚本:**
```sql
IF OBJECT_ID(N'Users', N'U') IS NULL
BEGIN
    CREATE TABLE Users (
        Id int PRIMARY KEY,
        Name nvarchar(100)
    );
END
GO

IF OBJECT_ID(N'GetUser', N'P') IS NULL
    EXEC('CREATE PROCEDURE GetUser AS SELECT 1;');
GO
ALTER PROCEDURE GetUser(@Id int) 
AS
SELECT * FROM Users WHERE Id = @Id;
GO
```

## 🛠️ 技术规格

- **目标框架**: .NET Framework 4.8
- **支持系统**: Windows 
- **SQL Server 版本**: 2008R2+ (部分功能需要 2012+)
- **文本编码**: UTF-8, GBK, UTF-16 等
- **许可证**: MIT

## 📦 下载和安装

1. 从 [Releases](https://github.com/470503790/SqlProcessorCommand/releases) 页面下载最新版本
2. 解压到任意目录
3. 确保系统已安装 .NET Framework 4.8
4. 在命令行中运行 `SqlProcessorCommand.exe`

## 🤝 贡献

欢迎提交 Issue 和 Pull Request 来：
- 补充更多幂等规则
- 改进正则表达式匹配
- 增加新的 SQL 操作支持
- 改善文档和示例

## 📄 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

---

感谢使用 SqlProcessorCommand！如有问题或建议，请通过 GitHub Issues 联系我们。