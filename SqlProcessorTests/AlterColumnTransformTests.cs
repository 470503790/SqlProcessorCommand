using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class AlterColumnTransformTests
    {
        private AlterColumnTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new AlterColumnTransform();
        }

        [TestMethod]
        public void CanHandle_ValidAlterTableAlterColumn_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] ALTER COLUMN [ExistingColumn] VARCHAR(100) NOT NULL;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterTableAlterColumnWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ALTER COLUMN ExistingColumn INT NULL;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterTableAlterColumnWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] ALTER COLUMN [ExistingColumn] DECIMAL(10,2) NOT NULL;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterTableAddColumn_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] ADD [NewColumn] VARCHAR(50);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanHandle_InvalidStatement_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE TABLE TestTable (ID INT);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_AlterTableAlterColumnWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] ALTER COLUMN [ExistingColumn] VARCHAR(100) NOT NULL;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[TestSchema].[TestTable]', N'ExistingColumn') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AlterTableAlterColumnWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ALTER COLUMN ExistingColumn INT NULL;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[TestTable]', N'ExistingColumn') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AlterTableAlterColumnWithBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] ALTER COLUMN [ExistingColumn] DECIMAL(10,2) NOT NULL;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[TestTable]', N'ExistingColumn') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "alter table testtable alter column existingcolumn varchar(50) null;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[testtable]', N'existingcolumn') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AlterColumnChangeType_HandlesCorrectly()
        {
            // Arrange
            var sql = "ALTER TABLE [dbo].[Orders] ALTER COLUMN [Total] DECIMAL(18,4) NOT NULL;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[Orders]', N'Total') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}