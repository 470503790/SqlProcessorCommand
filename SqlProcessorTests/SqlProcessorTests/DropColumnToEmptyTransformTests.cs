using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropColumnToEmptyTransformTests
    {
        private DropColumnToEmptyTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropColumnToEmptyTransform();
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
        public void CanHandle_AlterTableDropColumnWithoutSchema_ReturnsTrue()
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
        public void CanHandle_CreateTable_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE TABLE TestTable (ID INT);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropColumnStatement_ReturnsEmptyString()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] DROP COLUMN [OldColumn];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void Transform_DropColumnWithoutSchema_ReturnsEmptyString()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable DROP COLUMN OldColumn;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void Transform_ComplexDropColumnStatement_ReturnsEmptyString()
        {
            // Arrange
            var sql = @"ALTER TABLE [dbo].[Orders]
                       DROP COLUMN [LegacyColumn],
                                   [AnotherOldColumn];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}