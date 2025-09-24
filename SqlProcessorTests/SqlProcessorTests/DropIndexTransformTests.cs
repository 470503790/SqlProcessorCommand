using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropIndexTransformTests
    {
        private DropIndexTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropIndexTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropIndex_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP INDEX [IX_TestTable_Column] ON [TestSchema].[TestTable];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropIndexWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "DROP INDEX IX_TestTable_Column ON TestTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateIndex_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE INDEX [IX_TestTable_Column] ON [TestTable] ([Column]);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropIndexWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "DROP INDEX [IX_TestTable_Column] ON [TestSchema].[TestTable];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TestTable_Column' AND object_id = OBJECT_ID(N'[TestSchema].[TestTable]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropIndexWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "DROP INDEX IX_TestTable_Column ON TestTable;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TestTable_Column' AND object_id = OBJECT_ID(N'[dbo].[TestTable]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}