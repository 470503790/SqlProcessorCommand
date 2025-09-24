using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateTableTransformTests
    {
        private CreateTableTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateTableTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateTable_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TABLE [TestSchema].[TestTable] (ID INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100));";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateTableWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TABLE TestTable (ID INT, Name VARCHAR(50));";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateTableWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE TABLE [TestTable] (ID INT NOT NULL);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_DropTable_ReturnsFalse()
        {
            // Arrange
            var sql = "DROP TABLE [TestSchema].[TestTable];";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanHandle_InvalidStatement_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE PROCEDURE TestProc AS BEGIN SELECT 1; END";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateTableWithSchema_WrapsWithExistenceCheck()
        {
            // Arrange
            var sql = "CREATE TABLE [TestSchema].[TestTable] (ID INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100));";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestTable]', N'U') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateTableWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE TABLE TestTable (ID INT, Name VARCHAR(50));";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestTable]', N'U') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CreateTableWithBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE TABLE [TestTable] (ID INT NOT NULL, CreatedDate DATETIME DEFAULT GETDATE());";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestTable]', N'U') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "create table testtable (id int, name varchar(50));";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[testtable]', N'U') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }

        [TestMethod]
        public void Transform_ComplexTable_HandlesCorrectly()
        {
            // Arrange
            var sql = @"CREATE TABLE [dbo].[Orders] (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NULL"));
            Assert.IsTrue(result.Contains("BEGIN"));
            Assert.IsTrue(result.Contains(sql));
            Assert.IsTrue(result.Contains("END"));
        }
    }
}