# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-27

### Added
- Initial release of SqlProcessorCommand
- SQL Server 脚本幂等性转换工具
- 支持将常规升级脚本转换为可重复执行的安全脚本
- 命令行界面支持多种参数配置
- 库调用接口，可集成到其他项目

### Features
- **表管理**: CREATE TABLE, DROP TABLE（可配置丢弃或包装）
- **列管理**: ADD COLUMN, ALTER COLUMN, DROP COLUMN（可配置丢弃或包装）
- **索引管理**: CREATE INDEX, DROP INDEX
- **约束管理**: ADD CONSTRAINT, DROP CONSTRAINT
- **存储过程**: CREATE/ALTER PROCEDURE（跨版本兼容）
- **视图**: CREATE/ALTER VIEW（跨版本兼容）
- **触发器**: CREATE/ALTER TRIGGER
- **函数**: CREATE/ALTER FUNCTION
- **序列**: CREATE/ALTER/DROP SEQUENCE（SQL 2012+）
- **用户定义类型**: CREATE/DROP TYPE
- **架构**: CREATE SCHEMA
- **数据库角色和用户**: CREATE/DROP ROLE, CREATE/DROP USER
- **扩展属性**: sp_addextendedproperty/sp_updateextendedproperty
- **SQL Server 2014 兼容性支持**

### Technical Details
- 目标框架：.NET Framework 4.8
- 按 GO 分批处理 SQL 脚本
- 可插拔的转换管道架构
- 支持多种文本编码（UTF-8, GBK, UTF-16等）
- MIT 许可证

### Command Line Usage
```bash
SqlProcessorCommand -i upgrade.sql                           # 基本用法
SqlProcessorCommand -i upgrade.sql -n safe                   # 自定义后缀
SqlProcessorCommand -i upgrade.sql --keep-drop-table         # 保留 DROP TABLE
```

### Library Usage
```csharp
using SqlProcessorCommand;

var options = new SqlIdempotentProcessor.Options {
    DiscardDropTable = true,
    DiscardDropColumn = true
};
var processor = new SqlIdempotentProcessor(options);
string outputSql = processor.Transform(inputSql);
```

[1.0.0]: https://github.com/470503790/SqlProcessorCommand/releases/tag/v1.0.0