using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class AlterSequenceTransformTests
    {
        private AlterSequenceTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new AlterSequenceTransform();
        }

        [TestMethod]
        public void CanHandle_ValidAlterSequence_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER SEQUENCE [TestSchema].[TestSequence] RESTART WITH 1000;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterSequenceWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER SEQUENCE TestSequence INCREMENT BY 5;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateSequence_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE SEQUENCE [TestSchema].[TestSequence] START WITH 1;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_AlterSequenceWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "ALTER SEQUENCE [TestSchema].[TestSequence] RESTART WITH 1000;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestSequence]', N'SO') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AlterSequenceWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "ALTER SEQUENCE TestSequence INCREMENT BY 5;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestSequence]', N'SO') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}