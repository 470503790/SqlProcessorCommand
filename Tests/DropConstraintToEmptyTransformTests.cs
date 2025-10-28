using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropConstraintToEmptyTransformTests
    {
        private readonly DropConstraintToEmptyTransform _transform = new DropConstraintToEmptyTransform();

        [TestMethod]
        public void CanHandle_StandardSyntax_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE dbo.TestTable DROP CONSTRAINT FK_TestConstraint";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_BracketedSyntax_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_WithSemicolon_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint];";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_WithoutSchema_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE TestTable DROP CONSTRAINT FK_TestConstraint";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AddConstraint_ReturnsFalse()
        {
            // Arrange
            string sql = "ALTER TABLE TestTable ADD CONSTRAINT FK_Test FOREIGN KEY (ID) REFERENCES Other(ID)";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_ReturnsEmptyString()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [FK_TestConstraint]";
            
            // Act
            string result = _transform.Transform(sql);
            
            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithDF_ReturnsFalse()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [DF_TestTable_Column]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsFalse(result, "Default constraints with DF_ prefix should be excluded");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithDFDouble_ReturnsFalse()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[r_complaint_item] DROP CONSTRAINT [DF__r_complai__respo__1401A6ED]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsFalse(result, "Default constraints with DF__ prefix should be excluded");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithoutBrackets_ReturnsFalse()
        {
            // Arrange
            string sql = "ALTER TABLE dbo.TestTable DROP CONSTRAINT DF_TestTable_Status";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsFalse(result, "Default constraints without brackets should also be excluded");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintCaseInsensitive_ReturnsFalse()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [df_TestTable_Column]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsFalse(result, "Default constraint check should be case-insensitive");
        }
    }
}
