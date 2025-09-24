using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropUserTransformTests
    {
        private DropUserTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropUserTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropUser_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP USER [TestUser];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropUserWithoutBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP USER TestUser;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateUser_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE USER [TestUser] FOR LOGIN [TestLogin];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropUser_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "DROP USER [TestUser];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[TestUser]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropUserWithoutBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "DROP USER TestUser;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[TestUser]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}