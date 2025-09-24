#!/bin/bash

# Package release script for SqlProcessorCommand
# This script creates a release package for distribution

VERSION="1.0.0"
PACKAGE_NAME="SqlProcessorCommand-v${VERSION}"
RELEASE_DIR="./release"
BUILD_DIR="./bin/Release/net48"

echo "=== SqlProcessorCommand Release Packaging ==="
echo "Version: $VERSION"
echo "Package Name: $PACKAGE_NAME"
echo ""

# Clean and create release directory
if [ -d "$RELEASE_DIR" ]; then
    rm -rf "$RELEASE_DIR"
fi
mkdir -p "$RELEASE_DIR/$PACKAGE_NAME"

# Build the project
echo "Building project..."
dotnet build --configuration Release --no-restore

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo "✅ Build successful"

# Copy executable and dependencies
echo "Copying files..."
cp "$BUILD_DIR/SqlProcessorCommand.exe" "$RELEASE_DIR/$PACKAGE_NAME/"
cp "$BUILD_DIR/SqlProcessorCommand.exe.config" "$RELEASE_DIR/$PACKAGE_NAME/" 2>/dev/null || true

# Copy documentation
cp "README.md" "$RELEASE_DIR/$PACKAGE_NAME/"
cp "LICENSE" "$RELEASE_DIR/$PACKAGE_NAME/"
cp "CHANGELOG.md" "$RELEASE_DIR/$PACKAGE_NAME/"
cp "RELEASE_NOTES.md" "$RELEASE_DIR/$PACKAGE_NAME/"

# Create usage example
cat > "$RELEASE_DIR/$PACKAGE_NAME/USAGE_EXAMPLE.md" << 'EOF'
# SqlProcessorCommand 使用示例

## 快速开始

1. 准备一个 SQL 脚本文件，例如 `upgrade.sql`:
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

2. 运行命令生成幂等脚本:
```cmd
SqlProcessorCommand.exe -i upgrade.sql
```

3. 查看生成的 `upgrade.idempotent.sql` 文件，内容如下:
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

现在这个脚本可以安全地多次执行，不会因为对象已存在而报错！

## 更多选项

```cmd
# 自定义输出文件后缀
SqlProcessorCommand.exe -i upgrade.sql -n safe

# 保留 DROP 语句但包装为安全执行  
SqlProcessorCommand.exe -i upgrade.sql --keep-drop-table

# 查看所有选项
SqlProcessorCommand.exe --help
```
EOF

# Create a simple batch file for Windows users
cat > "$RELEASE_DIR/$PACKAGE_NAME/run_example.bat" << 'EOF'
@echo off
echo SqlProcessorCommand 示例运行
echo.
echo 使用方法:
echo   1. 将你的 SQL 文件放在同一目录下
echo   2. 运行: SqlProcessorCommand.exe -i 你的文件.sql
echo.
echo 示例:
echo   SqlProcessorCommand.exe -i upgrade.sql
echo.
pause
EOF

# Create directory listing
echo "Package contents:" > "$RELEASE_DIR/$PACKAGE_NAME/FILES.txt"
ls -la "$RELEASE_DIR/$PACKAGE_NAME/" >> "$RELEASE_DIR/$PACKAGE_NAME/FILES.txt"

echo ""
echo "✅ Package created successfully!"
echo "Location: $RELEASE_DIR/$PACKAGE_NAME/"
echo ""
echo "Files included:"
ls -la "$RELEASE_DIR/$PACKAGE_NAME/"

echo ""
echo "🎉 Ready for release! You can now:"
echo "   1. Create a ZIP archive of the $PACKAGE_NAME folder"
echo "   2. Upload it to GitHub Releases"
echo "   3. Tag the release as v$VERSION"