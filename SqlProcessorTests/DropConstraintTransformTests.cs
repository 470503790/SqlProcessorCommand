using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropConstraintTransformTests
    {
        private DropConstraintTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new DropConstraintTransform();
        }

        [TestMethod]
        public void CanHandle_ValidDropConstraint_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] DROP CONSTRAINT [FK_TestConstraint];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropConstraintWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable DROP CONSTRAINT FK_TestConstraint;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterTableAddConstraint_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable ADD CONSTRAINT FK_Test FOREIGN KEY (ID) REFERENCES Other(ID);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_DropConstraintWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "ALTER TABLE [TestSchema].[TestTable] DROP CONSTRAINT [FK_TestConstraint];";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_TestConstraint' AND parent_object_id = OBJECT_ID(N'[TestSchema].[TestTable]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_DropConstraintWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "ALTER TABLE TestTable DROP CONSTRAINT FK_TestConstraint;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_TestConstraint' AND parent_object_id = OBJECT_ID(N'[dbo].[TestTable]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithDF_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [DF_TestTable_Column];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result, "Default constraints with DF_ prefix should be excluded");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithDFDouble_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE [dbo].[r_complaint_item] DROP CONSTRAINT [DF__r_complai__respo__1401A6ED];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result, "Default constraints with DF__ prefix should be excluded");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithoutBrackets_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE dbo.TestTable DROP CONSTRAINT DF_TestTable_Status;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result, "Default constraints without brackets should also be excluded");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintCaseInsensitive_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [df_TestTable_Column];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result, "Default constraint check should be case-insensitive");
        }

        [TestMethod]
        public void CanHandle_ConstraintWithComplexName_ReturnsTrue()
        {
            // Arrange - Real world example from issue with complex auto-generated constraint name
            var sql = "ALTER TABLE [dbo].[ftweb_order] DROP CONSTRAINT [DF__ftweb_ord__pay_t__6017EE93]";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            // This is a default constraint (starts with DF__), so it should be excluded
            Assert.IsFalse(result, "Constraint DF__ftweb_ord__pay_t__6017EE93 is a default constraint and should be excluded");
        }

        [TestMethod]
        public void CanHandle_NonDefaultConstraintWithComplexName_ReturnsTrue()
        {
            // Arrange - Similar pattern but not a default constraint
            var sql = "ALTER TABLE [dbo].[ftweb_order] DROP CONSTRAINT [FK__ftweb_ord__pay_t__6017EE93]";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result, "FK constraint with complex name should be handled");
        }

        [TestMethod]
        public void Transform_NonDefaultConstraintWithComplexName_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "ALTER TABLE [dbo].[ftweb_order] DROP CONSTRAINT [FK__ftweb_ord__pay_t__6017EE93]";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__ftweb_ord__pay_t__6017EE93' AND parent_object_id = OBJECT_ID(N'[dbo].[ftweb_order]'))"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}