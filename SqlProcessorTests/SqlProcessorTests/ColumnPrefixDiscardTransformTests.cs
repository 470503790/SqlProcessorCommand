using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class ColumnPrefixDiscardTransformTests
    {
        private ColumnPrefixDiscardTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new ColumnPrefixDiscardTransform();
        }

        [TestMethod]
        public void CanHandle_AnyStatement_ReturnsFalse()
        {
            // This transform is designed to be disabled by default
            // Arrange
            var sql = "-- __DISCARD__ This should be discarded";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanHandle_RegularSQLStatement_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE TABLE TestTable (ID INT);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanHandle_EmptyString_ReturnsFalse()
        {
            // Arrange
            var sql = "";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_AnyInput_ReturnsEmptyString()
        {
            // Even though CanHandle returns false, Transform should return empty string as designed
            // Arrange
            var sql = "Any SQL statement here";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void Transform_DiscardMarkedStatement_ReturnsEmptyString()
        {
            // Arrange
            var sql = "-- __DISCARD__ This line should be removed";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void Transform_EmptyInput_ReturnsEmptyString()
        {
            // Arrange
            var sql = "";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}