using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropRoleTransformTests
    {
        private DropRoleTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropRoleTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropRole_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP ROLE [TestRole];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropRoleWithoutBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP ROLE TestRole;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateRole_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE ROLE [TestRole];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropRole_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "DROP ROLE [TestRole];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[TestRole]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropRoleWithoutBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "DROP ROLE TestRole;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF DATABASE_PRINCIPAL_ID(N'[TestRole]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}