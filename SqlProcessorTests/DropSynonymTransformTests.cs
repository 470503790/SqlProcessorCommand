using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropSynonymTransformTests
    {
        private DropSynonymTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropSynonymTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropSynonym_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP SYNONYM [TestSchema].[TestSynonym];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropSynonymWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP SYNONYM TestSynonym;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateSynonym_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE SYNONYM [TestSchema].[TestSynonym] FOR [TargetTable];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropSynonymWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "DROP SYNONYM [TestSchema].[TestSynonym];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestSynonym]', N'SN') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropSynonymWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "DROP SYNONYM TestSynonym;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestSynonym]', N'SN') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}