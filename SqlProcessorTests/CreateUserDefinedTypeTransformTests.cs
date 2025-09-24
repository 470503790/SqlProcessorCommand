using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateUserDefinedTypeTransformTests
    {
        private CreateUserDefinedTypeTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateUserDefinedTypeTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateTypeFromBaseType_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TYPE [TestSchema].[TestType] FROM VARCHAR(50);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateTypeAsTable_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TYPE [TestSchema].[TestTableType] AS TABLE (ID INT, Name VARCHAR(50));";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateTypeWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TYPE TestType FROM INT;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateTypeWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TYPE [TestType] FROM DECIMAL(10,2);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropType_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP TYPE [TestSchema].[TestType];";

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
        public void Transform_CreateTypeWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "CREATE TYPE [TestSchema].[TestType] FROM VARCHAR(50);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF TYPE_ID(N'[TestSchema].[TestType]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateTypeWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE TYPE TestType FROM INT;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF TYPE_ID(N'[dbo].[TestType]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateTableType_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE TYPE [dbo].[TestTableType] AS TABLE (ID INT IDENTITY(1,1), Name NVARCHAR(100) NOT NULL);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF TYPE_ID(N'[dbo].[TestTableType]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "create type testtype from varchar(20);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF TYPE_ID(N'[dbo].[testtype]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}