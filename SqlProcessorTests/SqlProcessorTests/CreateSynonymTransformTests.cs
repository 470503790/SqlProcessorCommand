using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateSynonymTransformTests
    {
        private CreateSynonymTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateSynonymTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateSynonym_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE SYNONYM [TestSchema].[TestSynonym] FOR [TargetSchema].[TargetTable];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateSynonymWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE SYNONYM TestSynonym FOR TargetTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropSynonym_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP SYNONYM [TestSchema].[TestSynonym];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateSynonymWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "CREATE SYNONYM [TestSchema].[TestSynonym] FOR [TargetSchema].[TargetTable];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestSynonym]', N'SN') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateSynonymWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE SYNONYM TestSynonym FOR TargetTable;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestSynonym]', N'SN') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}