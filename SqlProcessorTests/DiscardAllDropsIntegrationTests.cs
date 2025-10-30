using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DiscardAllDropsIntegrationTests
    {
        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropTable()
        {
            // Arrange
            string sql = "DROP TABLE [dbo].[TestTable]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropColumn()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP COLUMN [TestColumn]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropConstraint()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_Test]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropIndex()
        {
            // Arrange
            string sql = "DROP INDEX [IX_Test] ON [dbo].[TestTable]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropSchema()
        {
            // Arrange
            string sql = "DROP SCHEMA [TestSchema]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropSynonym()
        {
            // Arrange
            string sql = "DROP SYNONYM [dbo].[TestSynonym]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropSequence()
        {
            // Arrange
            string sql = "DROP SEQUENCE [dbo].[TestSequence]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropType()
        {
            // Arrange
            string sql = "DROP TYPE [dbo].[TestType]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropUser()
        {
            // Arrange
            string sql = "DROP USER [TestUser]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_DiscardsDropRole()
        {
            // Arrange
            string sql = "DROP ROLE [TestRole]";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim());
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_KeepsNonDropStatements()
        {
            // Arrange
            string sql = "CREATE TABLE [dbo].[NewTable] (ID INT)";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("CREATE TABLE"));
            Assert.IsTrue(result.Contains("NewTable"));
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_OverridesIndividualSettings()
        {
            // Arrange
            string sql = @"DROP TABLE [dbo].[TestTable]
GO
DROP INDEX [IX_Test] ON [dbo].[TestTable]
GO
ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_Test]
GO
ALTER TABLE [dbo].[TestTable] DROP COLUMN [TestColumn]";
            var options = new SqlIdempotentProcessor.Options 
            { 
                DiscardAllDrops = true,
                // These should be ignored when DiscardAllDrops is true
                DiscardDropTable = false,
                DiscardDropColumn = false,
                DiscardDropConstraint = false,
                DiscardDropIndex = false
            };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.AreEqual(string.Empty, result.Trim(), "All DROP statements should be discarded");
        }

        [TestMethod]
        public void Transform_DiscardAllDrops_WithMixedStatements()
        {
            // Arrange
            string sql = @"CREATE TABLE [dbo].[NewTable] (ID INT)
GO
DROP TABLE [dbo].[OldTable]
GO
DROP SCHEMA [TestSchema]
GO
DROP SYNONYM [dbo].[TestSynonym]
GO
CREATE PROCEDURE [dbo].[TestProc] AS SELECT 1";
            var options = new SqlIdempotentProcessor.Options { DiscardAllDrops = true };
            var processor = new SqlIdempotentProcessor(options);

            // Act
            string result = processor.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("CREATE TABLE"));
            Assert.IsTrue(result.Contains("CREATE PROCEDURE"));
            Assert.IsFalse(result.Contains("DROP TABLE"));
            Assert.IsFalse(result.Contains("DROP SCHEMA"));
            Assert.IsFalse(result.Contains("DROP SYNONYM"));
        }
    }
}
