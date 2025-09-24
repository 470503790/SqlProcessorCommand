using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateOrAlterProcedureTransformTests
    {
        private CreateOrAlterProcedureTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateOrAlterProcedureTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateOrAlterProcedure_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE OR ALTER PROCEDURE [TestSchema].[TestProcedure] AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateOrAlterProcedureWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE OR ALTER PROCEDURE TestProcedure AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateOrAlterProcedureWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE OR ALTER PROCEDURE [TestProcedure] AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateProcedureOnly_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE PROCEDURE TestProcedure AS BEGIN SELECT 1; END";

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
        public void Transform_CreateOrAlterProcedureWithSchema_ConvertsToCompatibleForm()
        {
            // Arrange
            var sql = "CREATE OR ALTER PROCEDURE [TestSchema].[TestProcedure] AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestProcedure]', N'P') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP PROCEDURE [TestSchema].[TestProcedure];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE PROCEDURE [TestSchema].[TestProcedure] AS BEGIN SELECT 1; END"));
        }

        [TestMethod]
        public void Transform_CreateOrAlterProcedureWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE OR ALTER PROCEDURE TestProcedure AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestProcedure]', N'P') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP PROCEDURE [dbo].[TestProcedure];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE PROCEDURE TestProcedure AS BEGIN SELECT 1; END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "create or alter procedure testproc as begin select 1; end";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[testproc]', N'P') IS NOT NULL"));
            Assert.IsTrue(result.Contains("CREATE procedure testproc as begin select 1; end"));
        }

        [TestMethod]
        public void Transform_WithParameters_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE OR ALTER PROCEDURE [dbo].[GetUser] @UserId INT, @UserName NVARCHAR(50) AS BEGIN SELECT * FROM Users WHERE Id = @UserId; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[GetUser]', N'P') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP PROCEDURE [dbo].[GetUser];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE PROCEDURE [dbo].[GetUser] @UserId INT, @UserName NVARCHAR(50)"));
        }
    }
}