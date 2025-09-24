using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateUserTransformTests
    {
        private CreateUserTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateUserTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateUser_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE USER [TestUser] FOR LOGIN [TestLogin];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateUserWithoutBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE USER TestUser FOR LOGIN TestLogin;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateUserWithDefaultSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE USER [TestUser] FOR LOGIN [TestLogin] WITH DEFAULT_SCHEMA = dbo;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropUser_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP USER [TestUser];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateUser_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "CREATE USER [TestUser] FOR LOGIN [TestLogin];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[TestUser]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateUserWithoutBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE USER TestUser FOR LOGIN TestLogin;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[TestUser]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}