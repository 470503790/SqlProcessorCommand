using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class AlterColumnTransformTests
    {
        private readonly AlterColumnTransform _transform = new AlterColumnTransform();

        [TestMethod]
        public void CanHandle_StandardSyntax_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE dbo.Users ALTER COLUMN Name NVARCHAR(100) NULL";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_BracketedSyntax_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[Users] ALTER COLUMN [Name] NVARCHAR(100) NULL";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_WithCollation_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[p_enquiry_item] ALTER COLUMN [p_enquiry_pic] nvarchar(max) COLLATE Chinese_PRC_CI_AS NULL";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_NoSchema_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE Users ALTER COLUMN Name NVARCHAR(100) NULL";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Transform_AddsColumnExistenceCheck()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[Users] ALTER COLUMN [Name] NVARCHAR(100) NULL";
            
            // Act
            string result = _transform.Transform(sql);
            
            // Assert
            StringAssert.Contains(result, "IF COL_LENGTH(N'[dbo].[Users]', N'Name') IS NOT NULL");
            StringAssert.Contains(result, "BEGIN");
            StringAssert.Contains(result, sql);
            StringAssert.Contains(result, "END");
        }

        [TestMethod]
        public void Transform_WithCollation_AddsColumnExistenceCheck()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[p_enquiry_item] ALTER COLUMN [p_enquiry_pic] nvarchar(max) COLLATE Chinese_PRC_CI_AS NULL";
            
            // Act
            string result = _transform.Transform(sql);
            
            // Assert
            StringAssert.Contains(result, "IF COL_LENGTH(N'[dbo].[p_enquiry_item]', N'p_enquiry_pic') IS NOT NULL");
            StringAssert.Contains(result, "BEGIN");
            StringAssert.Contains(result, sql);
            StringAssert.Contains(result, "END");
        }
    }
}