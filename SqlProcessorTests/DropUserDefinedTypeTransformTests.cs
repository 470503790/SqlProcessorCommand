using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropUserDefinedTypeTransformTests
    {
        private DropUserDefinedTypeTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropUserDefinedTypeTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropType_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP TYPE [TestSchema].[TestType];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropTypeWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP TYPE TestType;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateType_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE TYPE [TestSchema].[TestType] FROM VARCHAR(50);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropTypeWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "DROP TYPE [TestSchema].[TestType];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF TYPE_ID(N'[TestSchema].[TestType]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropTypeWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "DROP TYPE TestType;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF TYPE_ID(N'[dbo].[TestType]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}