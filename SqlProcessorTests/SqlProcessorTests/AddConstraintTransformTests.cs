using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class AddConstraintTransformTests
    {
        private AddConstraintTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new AddConstraintTransform();
        }

        [TestMethod]
        public void CanHandle_ValidAlterTableAddConstraint_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] ADD CONSTRAINT [FK_TestConstraint] FOREIGN KEY ([Column1]) REFERENCES [OtherTable]([ID]);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AddConstraintWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ADD CONSTRAINT PK_TestTable PRIMARY KEY (ID);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AddConstraintCheck_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] ADD CONSTRAINT [CK_TestTable_Status] CHECK ([Status] IN ('Active', 'Inactive'));";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterTableDropConstraint_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE [TestTable] DROP CONSTRAINT [FK_TestConstraint];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_AddConstraintWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] ADD CONSTRAINT [FK_TestConstraint] FOREIGN KEY ([Column1]) REFERENCES [OtherTable]([ID]);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS ("));
            Assert.IsTrue(result.Contains("SELECT 1 FROM sys.objects WHERE name = N'FK_TestConstraint' AND parent_object_id = OBJECT_ID(N'[TestSchema].[TestTable]')"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_AddConstraintWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ADD CONSTRAINT PK_TestTable PRIMARY KEY (ID);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF NOT EXISTS ("));
            Assert.IsTrue(result.Contains("SELECT 1 FROM sys.objects WHERE name = N'PK_TestTable' AND parent_object_id = OBJECT_ID(N'[dbo].[TestTable]')"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}