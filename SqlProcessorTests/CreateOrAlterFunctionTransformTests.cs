using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateOrAlterFunctionTransformTests
    {
        private CreateOrAlterFunctionTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateOrAlterFunctionTransform();
        }

        [TestMethod]
        public void CanHandle_ValidAlterFunction_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER FUNCTION [TestSchema].[TestFunction]() RETURNS INT AS BEGIN RETURN 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterFunctionWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER FUNCTION TestFunction(@Param INT) RETURNS INT AS BEGIN RETURN @Param; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterFunctionWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER FUNCTION [TestFunction] (@Param INT) RETURNS INT AS BEGIN RETURN @Param; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateFunctionOnly_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE FUNCTION TestFunction() RETURNS INT AS BEGIN RETURN 1; END";

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
        public void Transform_AlterFunctionWithSchema_ConvertsToCompatibleForm()
        {
            // Arrange
            var sql = "ALTER FUNCTION [TestSchema].[TestFunction]() RETURNS INT AS BEGIN RETURN 1; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestFunction]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP FUNCTION [TestSchema].[TestFunction];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE FUNCTION [TestSchema].[TestFunction]() RETURNS INT AS BEGIN RETURN 1; END"));
        }

        [TestMethod]
        public void Transform_AlterFunctionWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "ALTER FUNCTION TestFunction(@Param INT) RETURNS INT AS BEGIN RETURN @Param; END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestFunction]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP FUNCTION [dbo].[TestFunction];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE FUNCTION TestFunction(@Param INT) RETURNS INT AS BEGIN RETURN @Param; END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "alter function testfunc() returns int as begin return 1; end";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[testfunc]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("CREATE function testfunc() returns int as begin return 1; end"));
        }

        [TestMethod]
        public void Transform_WithParameters_HandlesCorrectly()
        {
            // Arrange
            var sql = "ALTER FUNCTION [dbo].[GetUserCount] (@UserId INT, @UserName NVARCHAR(50)) RETURNS INT AS BEGIN RETURN (SELECT COUNT(*) FROM Users WHERE Id = @UserId); END";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[GetUserCount]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP FUNCTION [dbo].[GetUserCount];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE FUNCTION [dbo].[GetUserCount] (@UserId INT, @UserName NVARCHAR(50)) RETURNS INT"));
        }

        [TestMethod]
        public void Transform_TableValuedFunction_HandlesCorrectly()
        {
            // Arrange
            var sql = "ALTER FUNCTION [dbo].[GetUsers]() RETURNS TABLE AS RETURN (SELECT * FROM Users);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[GetUsers]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP FUNCTION [dbo].[GetUsers];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE FUNCTION [dbo].[GetUsers]() RETURNS TABLE"));
        }
    }
}
