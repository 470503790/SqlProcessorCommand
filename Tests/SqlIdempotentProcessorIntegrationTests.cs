using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class SqlIdempotentProcessorIntegrationTests
    {
        [TestMethod]
        public void Transform_DropConstraint_DiscardsWhenOptionIsTrue()
        {
            // Arrange
            string sql = @"ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint]
GO";
            var options = new SqlIdempotentProcessor.Options { DiscardDropConstraint = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DropConstraint_KeepsWhenOptionIsFalse()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint]";
            var options = new SqlIdempotentProcessor.Options { DiscardDropConstraint = false };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF EXISTS"));
            Assert.IsTrue(result.Contains("sys.objects"));
            Assert.IsTrue(result.Contains("DROP CONSTRAINT"));
        }

        [TestMethod]
        public void Transform_DropIndex_DiscardsWhenOptionIsTrue()
        {
            // Arrange
            string sql = @"DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable]
GO";
            var options = new SqlIdempotentProcessor.Options { DiscardDropIndex = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DropIndex_KeepsWhenOptionIsFalse()
        {
            // Arrange
            string sql = "DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable]";
            var options = new SqlIdempotentProcessor.Options { DiscardDropIndex = false };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF EXISTS"));
            Assert.IsTrue(result.Contains("sys.indexes"));
            Assert.IsTrue(result.Contains("DROP INDEX"));
        }

        [TestMethod]
        public void Transform_MultipleDropStatements_DiscardsAllByDefault()
        {
            // Arrange
            string sql = @"ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint]
GO
DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable]
GO
DROP TABLE [dbo].[OldTable]
GO
ALTER TABLE [dbo].[TestTable] DROP COLUMN [ObsoleteColumn]
GO";
            var options = new SqlIdempotentProcessor.Options(); // All defaults are true (discard)
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_MixedStatements_DiscardsDropsKeepsOthers()
        {
            // Arrange
            string sql = @"CREATE TABLE [dbo].[NewTable] (ID INT)
GO
DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable]
GO
DROP TABLE [dbo].[OldTable]
GO
CREATE PROCEDURE [dbo].[TestProc] AS SELECT 1
GO";
            var options = new SqlIdempotentProcessor.Options(); // All defaults
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("CREATE TABLE"));
            Assert.IsTrue(result.Contains("CREATE PROCEDURE"));
            Assert.IsFalse(result.Contains("DROP INDEX"));
            Assert.IsFalse(result.Contains("DROP TABLE"));
        }

        [TestMethod]
        public void Transform_AlterFunction_ConvertsToDropAndCreate()
        {
            // Arrange
            string sql = @"ALTER FUNCTION [dbo].[TestFunction]() RETURNS INT AS BEGIN RETURN 1; END
GO";
            var options = new SqlIdempotentProcessor.Options();
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestFunction]') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP FUNCTION [dbo].[TestFunction];"));
            Assert.IsTrue(result.Contains("CREATE FUNCTION [dbo].[TestFunction]() RETURNS INT AS BEGIN RETURN 1; END"));
            Assert.IsFalse(result.Contains("ALTER FUNCTION"));
        }

        [TestMethod]
        public void Transform_DropDefaultConstraint_IsNotDiscarded()
        {
            // Arrange
            string sql = @"ALTER TABLE [dbo].[r_complaint_item] DROP CONSTRAINT [DF__r_complai__respo__1401A6ED]
GO";
            var options = new SqlIdempotentProcessor.Options { DiscardDropConstraint = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            // Default constraints should NOT be discarded even with DiscardDropConstraint = true
            Assert.IsTrue(result.Contains("DROP CONSTRAINT"), "Default constraint drop should not be discarded");
            Assert.IsTrue(result.Contains("DF__r_complai__respo__1401A6ED"), "Default constraint name should be preserved");
        }

        [TestMethod]
        public void Transform_DropDefaultConstraintWithKeepOption_IsNotWrapped()
        {
            // Arrange
            string sql = @"ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [DF_TestTable_Status]
GO";
            var options = new SqlIdempotentProcessor.Options { DiscardDropConstraint = false };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            // Default constraints should NOT be wrapped even with DiscardDropConstraint = false
            Assert.IsTrue(result.Contains("DROP CONSTRAINT"), "Default constraint drop should be present");
            Assert.IsFalse(result.Contains("IF EXISTS"), "Default constraint should not be wrapped with IF EXISTS");
        }

        [TestMethod]
        public void Transform_DropNonDefaultConstraint_IsDiscarded()
        {
            // Arrange
            string sql = @"ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint]
GO";
            var options = new SqlIdempotentProcessor.Options { DiscardDropConstraint = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim(), "Non-default constraint should be discarded");
        }

        [TestMethod]
        public void Transform_MixedConstraints_HandlesCorrectly()
        {
            // Arrange
            string sql = @"ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint]
GO
ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [DF_TestTable_Status]
GO
ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [CK_TestConstraint]
GO";
            var options = new SqlIdempotentProcessor.Options { DiscardDropConstraint = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            // FK and CK should be discarded, DF should be kept
            Assert.IsFalse(result.Contains("FK_TestConstraint"), "FK constraint should be discarded");
            Assert.IsFalse(result.Contains("CK_TestConstraint"), "CK constraint should be discarded");
            Assert.IsTrue(result.Contains("DF_TestTable_Status"), "Default constraint should NOT be discarded");
        }
    }
}
