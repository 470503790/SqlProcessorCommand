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
    }
}
