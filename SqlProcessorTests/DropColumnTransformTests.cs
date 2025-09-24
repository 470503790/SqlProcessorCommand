using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropColumnTransformTests
    {
        private DropColumnTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropColumnTransform();
        }

        [TestMethod]
        public void CanHandle_ValidAlterTableDropColumn_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] DROP COLUMN [OldColumn];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropColumnWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable DROP COLUMN OldColumn;";

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
        public void Transform_DropColumnWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] DROP COLUMN [OldColumn];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[TestSchema].[TestTable]', N'OldColumn') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropColumnWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable DROP COLUMN OldColumn;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF COL_LENGTH(N'[dbo].[TestTable]', N'OldColumn') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}