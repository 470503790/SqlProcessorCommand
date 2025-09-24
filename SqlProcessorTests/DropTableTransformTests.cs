using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropTableTransformTests
    {
        private DropTableTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropTableTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropTableStatement_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP TABLE [TestSchema].[TestTable];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropTableWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP TABLE TestTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropTableWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP TABLE [TestTable];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
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
        public void CanHandle_EmptyString_ReturnsFalse()
        {
            // Arrange
            var sql = "";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropTableWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "DROP TABLE [TestSchema].[TestTable];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestTable]', N'U') IS NOT NULL"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_DropTableWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "DROP TABLE TestTable;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestTable]', N'U') IS NOT NULL"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_DropTableWithBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "DROP TABLE [TestTable];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestTable]', N'U') IS NOT NULL"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "drop table testtable;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[testtable]', N'U') IS NOT NULL"));
            Assert.IsTrue(result.Contains(sql));
        }
    }
}