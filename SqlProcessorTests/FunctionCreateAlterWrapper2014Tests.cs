using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class FunctionCreateAlterWrapper2014Tests
    {
        private FunctionCreateAlterWrapper2014 _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new FunctionCreateAlterWrapper2014();
        }

        [TestMethod]
        public void CanHandle_ValidCreateFunction_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE FUNCTION [TestSchema].[TestFunction]() RETURNS INT AS BEGIN RETURN 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateFunctionWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE FUNCTION TestFunction(@Param INT) RETURNS INT AS BEGIN RETURN @Param; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateTableFunction_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE FUNCTION [dbo].[GetUsers]() RETURNS TABLE AS RETURN (SELECT * FROM Users);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterFunction_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER FUNCTION [TestSchema].[TestFunction]() RETURNS INT AS BEGIN RETURN 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateFunctionWithSchema_ConvertsToCompatibleForm()
        {
            // Arrange
            var sql = "CREATE FUNCTION [TestSchema].[TestFunction]() RETURNS INT AS BEGIN RETURN 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestFunction]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP FUNCTION [TestSchema].[TestFunction];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_CreateFunctionWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE FUNCTION TestFunction(@Param INT) RETURNS INT AS BEGIN RETURN @Param; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestFunction]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP FUNCTION [dbo].[TestFunction];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }
    }
}