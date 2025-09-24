using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class AddColumnTransformTests
    {
        private AddColumnTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new AddColumnTransform();
        }

        [TestMethod]
        public void CanHandle_ValidAlterTableAddColumn_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] ADD [NewColumn] VARCHAR(50) NULL;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterTableAddColumnWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ADD NewColumn INT NOT NULL;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterTableAddColumnWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] ADD [NewColumn] DATETIME DEFAULT GETDATE();";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterTableDropColumn_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] DROP COLUMN [OldColumn];";

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
        public void Transform_AlterTableAddColumnWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] ADD [NewColumn] VARCHAR(50) NULL;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[TestSchema].[TestTable]', N'NewColumn') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AlterTableAddColumnWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ADD NewColumn INT NOT NULL;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[TestTable]', N'NewColumn') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AlterTableAddColumnWithBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] ADD [NewColumn] DATETIME DEFAULT GETDATE();";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[TestTable]', N'NewColumn') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "alter table testtable add newcolumn int null;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[testtable]', N'newcolumn') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AddColumnWithConstraint_HandlesCorrectly()
        {
            // Arrange
            var sql = "ALTER TABLE [dbo].[Users] ADD [Email] NVARCHAR(255) NOT NULL CONSTRAINT DF_Users_Email DEFAULT '';";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[Users]', N'Email') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AddColumnWithIdentity_HandlesCorrectly()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ADD NewId INT IDENTITY(1,1) NOT NULL;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[TestTable]', N'NewId') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}