using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class TriggerCreateAlterWrapper2014Tests
    {
        private TriggerCreateAlterWrapper2014 _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new TriggerCreateAlterWrapper2014();
        }

        [TestMethod]
        public void CanHandle_ValidCreateTrigger_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TRIGGER [TestSchema].[TestTrigger] ON [TestTable] AFTER INSERT AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateTriggerWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TRIGGER TestTrigger ON TestTable AFTER UPDATE AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateOrAlterTrigger_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE OR ALTER TRIGGER [TestSchema].[TestTrigger] ON [TestTable] AFTER INSERT AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateTriggerWithSchema_ConvertsToCompatibleForm()
        {
            // Arrange
            var sql = "CREATE TRIGGER [TestSchema].[TestTrigger] ON [TestTable] AFTER INSERT AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestTrigger]', N'TR') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP TRIGGER [TestSchema].[TestTrigger];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_CreateTriggerWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE TRIGGER TestTrigger ON TestTable AFTER UPDATE AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestTrigger]', N'TR') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP TRIGGER [dbo].[TestTrigger];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }
    }
}