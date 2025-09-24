using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateSchemaTransformTests
    {
        private CreateSchemaTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateSchemaTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE SCHEMA [TestSchema];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateSchemaWithoutBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE SCHEMA TestSchema;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateSchemaWithAuthorization_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE SCHEMA [TestSchema] AUTHORIZATION [dbo];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropSchema_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP SCHEMA [TestSchema];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "CREATE SCHEMA [TestSchema];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF SCHEMA_ID(N'[TestSchema]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateSchemaWithAuthorization_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE SCHEMA TestSchema AUTHORIZATION dbo;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF SCHEMA_ID(N'[TestSchema]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}