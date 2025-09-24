using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropSequenceTransformTests
    {
        private DropSequenceTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropSequenceTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropSequence_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP SEQUENCE [TestSchema].[TestSequence];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropSequenceWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP SEQUENCE TestSequence;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropSequenceWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP SEQUENCE [TestSequence];";

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
        public void CanHandle_InvalidStatement_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP TABLE TestTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropSequenceWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "DROP SEQUENCE [TestSchema].[TestSequence];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestSequence]', N'SO') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropSequenceWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "DROP SEQUENCE TestSequence;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestSequence]', N'SO') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropSequenceWithBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "DROP SEQUENCE [TestSequence];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestSequence]', N'SO') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "drop sequence testsequence;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[testsequence]', N'SO') IS NOT NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}