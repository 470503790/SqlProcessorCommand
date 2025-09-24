using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateSequenceTransformTests
    {
        private CreateSequenceTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateSequenceTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateSequence_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE SEQUENCE [TestSchema].[TestSequence] START WITH 1 INCREMENT BY 1;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateSequenceWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE SEQUENCE TestSequence START WITH 1 INCREMENT BY 1;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateSequenceWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE SEQUENCE [TestSequence] AS INT START WITH 1;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropSequence_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP SEQUENCE [TestSchema].[TestSequence];";

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
        public void Transform_CreateSequenceWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "CREATE SEQUENCE [TestSchema].[TestSequence] START WITH 1 INCREMENT BY 1;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestSequence]', N'SO') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateSequenceWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE SEQUENCE TestSequence START WITH 1 INCREMENT BY 1;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestSequence]', N'SO') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateSequenceWithBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE SEQUENCE [TestSequence] AS INT START WITH 1;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestSequence]', N'SO') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "create sequence testsequence start with 1;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[testsequence]', N'SO') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_ComplexSequenceDefinition_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE SEQUENCE [dbo].[OrderNumberSequence] AS BIGINT START WITH 1000 INCREMENT BY 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 10;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[OrderNumberSequence]', N'SO') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}