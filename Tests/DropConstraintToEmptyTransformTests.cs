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
    }
}
