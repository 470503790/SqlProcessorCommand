using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropSchemaTransformTests
    {
        private DropSchemaTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropSchemaTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP SCHEMA [TestSchema];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropSchemaWithoutBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP SCHEMA TestSchema;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateSchema_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE SCHEMA [TestSchema];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "DROP SCHEMA [TestSchema];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF SCHEMA_ID(N'[TestSchema]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropSchemaWithoutBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "DROP SCHEMA TestSchema;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF SCHEMA_ID(N'[TestSchema]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}