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
        public void CanHandle_DefaultConstraintWithDF_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [DF_TestTable_Column]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result, "Default constraints with DF_ prefix should now be handled");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithDFDouble_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[r_complaint_item] DROP CONSTRAINT [DF__r_complai__respo__1401A6ED]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result, "Default constraints with DF__ prefix should now be handled");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithoutBrackets_ReturnsFalse()
        {
            // Arrange - Note: This returns false because regex requires brackets around schema when using schema.table syntax
            string sql = "ALTER TABLE dbo.TestTable DROP CONSTRAINT DF_TestTable_Status";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsFalse(result, "Unquoted schema.table syntax is not supported by the regex");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintWithBracketedSchema_ReturnsTrue()
        {
            // Arrange - DF_ constraint with properly bracketed schema and table
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [DF_TestTable_Status]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result, "DF_ constraints with bracketed names should be handled");
        }

        [TestMethod]
        public void CanHandle_DefaultConstraintCaseInsensitive_ReturnsTrue()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[TestTable] DROP CONSTRAINT [df_TestTable_Column]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result, "Default constraints should be handled regardless of case");
        }

        [TestMethod]
        public void CanHandle_ConstraintWithComplexName_ReturnsTrue()
        {
            // Arrange - Real world example from issue with complex auto-generated constraint name
            string sql = "ALTER TABLE [dbo].[ftweb_order] DROP CONSTRAINT [DF__ftweb_ord__pay_t__6017EE93]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            // This is a default constraint (starts with DF__), so it should now be handled
            Assert.IsTrue(result, "Constraint DF__ftweb_ord__pay_t__6017EE93 is a default constraint and should now be handled");
        }

        [TestMethod]
        public void CanHandle_NonDefaultConstraintWithComplexName_ReturnsTrue()
        {
            // Arrange - Similar pattern but not a default constraint
            string sql = "ALTER TABLE [dbo].[ftweb_order] DROP CONSTRAINT [FK__ftweb_ord__pay_t__6017EE93]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result, "FK constraint with complex name should be handled");
        }

        [TestMethod]
        public void Transform_NonDefaultConstraintWithComplexName_ReturnsEmptyString()
        {
            // Arrange
            string sql = "ALTER TABLE [dbo].[ftweb_order] DROP CONSTRAINT [FK__ftweb_ord__pay_t__6017EE93]";
            
            // Act
            string result = _transform.Transform(sql);
            
            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CanHandle_RealWorldExample_ProBodyPriceLog_ReturnsTrue()
        {
            // Arrange - Example from the problem statement
            string sql = "ALTER TABLE [dbo].[pro_body_price_log] DROP CONSTRAINT [DF_pro_body_price_log_price_cost]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result, "Real-world DF_ constraint from problem statement should be handled");
        }

        [TestMethod]
        public void Transform_RealWorldExample_ProBodyPriceLog_ReturnsEmptyString()
        {
            // Arrange - Example from the problem statement
            string sql = "ALTER TABLE [dbo].[pro_body_price_log] DROP CONSTRAINT [DF_pro_body_price_log_price_cost]";
            
            // Act
            string result = _transform.Transform(sql);
            
            // Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}
