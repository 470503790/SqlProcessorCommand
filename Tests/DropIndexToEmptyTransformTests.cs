using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class DropIndexToEmptyTransformTests
    {
        private readonly DropIndexToEmptyTransform _transform = new DropIndexToEmptyTransform();

        [TestMethod]
        public void CanHandle_StandardSyntax_ReturnsTrue()
        {
            // Arrange
            string sql = "DROP INDEX IX_TestTable_Column ON dbo.TestTable";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_BracketedSyntax_ReturnsTrue()
        {
            // Arrange
            string sql = "DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable]";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_WithSemicolon_ReturnsTrue()
        {
            // Arrange
            string sql = "DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable];";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_WithoutSchema_ReturnsTrue()
        {
            // Arrange
            string sql = "DROP INDEX IX_TestTable_Column ON TestTable";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateIndex_ReturnsFalse()
        {
            // Arrange
            string sql = "CREATE INDEX [IX_TestTable_Column] ON [TestTable] ([Column])";
            
            // Act
            bool result = _transform.CanHandle(sql);
            
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_ReturnsEmptyString()
        {
            // Arrange
            string sql = "DROP INDEX [IX_TestTable_Column] ON [dbo].[TestTable]";
            
            // Act
            string result = _transform.Transform(sql);
            
            // Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}
