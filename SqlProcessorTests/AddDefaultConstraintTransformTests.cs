using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class AddDefaultConstraintTransformTests
    {
        private AddDefaultConstraintTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new AddDefaultConstraintTransform();
        }

        [TestMethod]
        public void CanHandle_ValidAddDefaultConstraint_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] ADD CONSTRAINT [DF_TestTable_Status] DEFAULT ('Active') FOR [Status];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AddDefaultConstraintWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ADD CONSTRAINT DF_TestTable_CreatedDate DEFAULT (GETDATE()) FOR CreatedDate;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AddForeignKeyConstraint_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] ADD CONSTRAINT [FK_TestTable_Other] FOREIGN KEY ([OtherId]) REFERENCES [OtherTable]([Id]);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_AddDefaultConstraintWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] ADD CONSTRAINT [DF_TestTable_Status] DEFAULT ('Active') FOR [Status];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS ("));
            Assert.IsTrue(result.Contains("FROM sys.default_constraints dc"));
            Assert.IsTrue(result.Contains("INNER JOIN sys.columns c"));
            Assert.IsTrue(result.Contains("WHERE dc.name = N'DF_TestTable_Status'"));
            Assert.IsTrue(result.Contains("AND dc.parent_object_id = OBJECT_ID(N'[TestSchema].[TestTable]')"));
            Assert.IsTrue(result.Contains("AND c.name = N'Status'"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AddDefaultConstraintWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ADD CONSTRAINT DF_TestTable_CreatedDate DEFAULT (GETDATE()) FOR CreatedDate;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS ("));
            Assert.IsTrue(result.Contains("WHERE dc.name = N'DF_TestTable_CreatedDate'"));
            Assert.IsTrue(result.Contains("AND dc.parent_object_id = OBJECT_ID(N'[dbo].[TestTable]')"));
            Assert.IsTrue(result.Contains("AND c.name = N'CreatedDate'"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}