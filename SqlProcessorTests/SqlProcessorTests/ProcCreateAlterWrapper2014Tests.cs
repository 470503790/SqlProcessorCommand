using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class ProcCreateAlterWrapper2014Tests
    {
        private ProcCreateAlterWrapper2014 _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new ProcCreateAlterWrapper2014();
        }

        [TestMethod]
        public void CanHandle_ValidCreateProcedure_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE PROCEDURE [TestSchema].[TestProcedure] AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateProcedureWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE PROCEDURE TestProcedure AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateOrAlterProcedure_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE OR ALTER PROCEDURE [TestSchema].[TestProcedure] AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateProcedureWithSchema_ConvertsToCompatibleForm()
        {
            // Arrange
            var sql = "CREATE PROCEDURE [TestSchema].[TestProcedure] AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestProcedure]', N'P') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP PROCEDURE [TestSchema].[TestProcedure];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_CreateProcedureWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE PROCEDURE TestProcedure AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestProcedure]', N'P') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP PROCEDURE [dbo].[TestProcedure];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }
    }
}