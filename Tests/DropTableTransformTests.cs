using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropTableTransformTests
    {
        private readonly DropTableTransform _transform = new DropTableTransform();

        [TestMethod]
        public void CanHandle_StandardSyntax_ReturnsTrue()
        {
            // Arrange
            string sql = "DROP TABLE dbo.Users";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_BracketedSyntax_ReturnsTrue()
        {
            // Arrange
            string sql = "DROP TABLE [dbo].[Users]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_WithSemicolon_ReturnsTrue()
        {
            // Arrange
            string sql = "DROP TABLE [dbo].[Users];";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Transform_AddsTableExistenceCheck()
        {
            // Arrange
            string sql = "DROP TABLE [dbo].[Users]";
            
            // Act
            string result = _transform.Transform(sql);
            
            // Assert
            StringAssert.Contains(result, "IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL");
            StringAssert.Contains(result, sql);
        }

        [TestMethod]
        public void Transform_DefaultSchemaHandled()
        {
            // Arrange
            string sql = "DROP TABLE Users";
            
            // Act
            string result = _transform.Transform(sql);
            
            // Assert
            StringAssert.Contains(result, "IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL");
        }
    }
}