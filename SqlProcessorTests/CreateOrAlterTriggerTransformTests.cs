using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateOrAlterTriggerTransformTests
    {
        private CreateOrAlterTriggerTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateOrAlterTriggerTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateOrAlterTrigger_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE OR ALTER TRIGGER [TestSchema].[TestTrigger] ON [TestTable] AFTER INSERT AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateOrAlterTriggerWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE OR ALTER TRIGGER TestTrigger ON TestTable AFTER UPDATE AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateTriggerOnly_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE TRIGGER TestTrigger ON TestTable AFTER INSERT AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateOrAlterTriggerWithSchema_ConvertsToCompatibleForm()
        {
            // Arrange
            var sql = "CREATE OR ALTER TRIGGER [TestSchema].[TestTrigger] ON [TestTable] AFTER INSERT AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestTrigger]', N'TR') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP TRIGGER [TestSchema].[TestTrigger];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE TRIGGER [TestSchema].[TestTrigger] ON [TestTable] AFTER INSERT AS BEGIN SELECT 1; END"));
        }

        [TestMethod]
        public void Transform_CreateOrAlterTriggerWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE OR ALTER TRIGGER TestTrigger ON TestTable AFTER UPDATE AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestTrigger]', N'TR') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP TRIGGER [dbo].[TestTrigger];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE TRIGGER TestTrigger ON TestTable AFTER UPDATE AS BEGIN SELECT 1; END"));
        }
    }
}