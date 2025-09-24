using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateIndexTransformTests
    {
        private CreateIndexTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateIndexTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateIndex_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE INDEX [IX_TestTable_Column] ON [TestSchema].[TestTable] ([Column]);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateUniqueIndex_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE UNIQUE INDEX [IX_TestTable_Column] ON [TestSchema].[TestTable] ([Column]);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateClusteredIndex_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE CLUSTERED INDEX [IX_TestTable_Column] ON [TestTable] ([Column]);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateNonClusteredIndex_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE NONCLUSTERED INDEX [IX_TestTable_Column] ON [TestTable] ([Column]);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropIndex_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP INDEX [IX_TestTable_Column] ON [TestTable];";

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
        public void Transform_CreateIndexWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "CREATE INDEX [IX_TestTable_Column] ON [TestSchema].[TestTable] ([Column]);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TestTable_Column' AND object_id = OBJECT_ID(N'[TestSchema].[TestTable]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateIndexWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE INDEX IX_TestTable_Column ON TestTable (Column);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TestTable_Column' AND object_id = OBJECT_ID(N'[dbo].[TestTable]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateUniqueIndex_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_Email' AND object_id = OBJECT_ID(N'[dbo].[Users]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateClusteredIndex_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE CLUSTERED INDEX [IX_TestTable_ID] ON [TestTable] ([ID]);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TestTable_ID' AND object_id = OBJECT_ID(N'[dbo].[TestTable]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "create index ix_testtable_column on testtable (column);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_testtable_column' AND object_id = OBJECT_ID(N'[dbo].[testtable]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_MultiColumnIndex_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE NONCLUSTERED INDEX [IX_Orders_CustomerDate] ON [dbo].[Orders] ([CustomerId], [OrderDate]) INCLUDE ([Total]);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_CustomerDate' AND object_id = OBJECT_ID(N'[dbo].[Orders]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}