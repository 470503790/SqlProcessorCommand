using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateRoleTransformTests
    {
        private CreateRoleTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateRoleTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateRole_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE ROLE [TestRole];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateRoleWithoutBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE ROLE TestRole;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateRoleWithOwnership_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE ROLE [TestRole] AUTHORIZATION [dbo];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropRole_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP ROLE [TestRole];";

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
        public void Transform_CreateRoleWithBrackets_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "CREATE ROLE [TestRole];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[TestRole]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateRoleWithoutBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE ROLE TestRole;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[TestRole]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateRoleWithAuthorization_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE ROLE [ApplicationRole] AUTHORIZATION [dbo];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[ApplicationRole]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "create role testrole;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[testrole]') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}