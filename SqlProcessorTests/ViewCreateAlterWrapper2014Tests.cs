using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class ViewCreateAlterWrapper2014Tests
    {
        private ViewCreateAlterWrapper2014 _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new ViewCreateAlterWrapper2014();
        }

        [TestMethod]
        public void CanHandle_ValidCreateView_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE VIEW [TestSchema].[TestView] AS SELECT * FROM TestTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateViewWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE VIEW TestView AS SELECT * FROM TestTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateViewWithBrackets_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE VIEW [TestView] AS SELECT ID, Name FROM Users;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_AlterView_ReturnsFalse()
        {
            // Arrange
            var sql = "ALTER VIEW [TestSchema].[TestView] AS SELECT * FROM TestTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanHandle_InvalidStatement_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE TABLE TestTable (ID INT);";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateViewWithSchema_ConvertsToCompatibleForm()
        {
            // Arrange
            var sql = "CREATE VIEW [TestSchema].[TestView] AS SELECT * FROM TestTable;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestView]', N'V') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP VIEW [TestSchema].[TestView];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_CreateViewWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE VIEW TestView AS SELECT * FROM TestTable;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestView]', N'V') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP VIEW [dbo].[TestView];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_CreateViewWithBrackets_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE VIEW [TestView] AS SELECT ID, Name FROM Users WHERE Active = 1;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestView]', N'V') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP VIEW [dbo].[TestView];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_CaseInsensitive_HandlesCorrectly()
        {
            // Arrange
            var sql = "create view testview as select * from testtable;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[testview]', N'V') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP VIEW [dbo].[testview];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }

        [TestMethod]
        public void Transform_ComplexView_HandlesCorrectly()
        {
            // Arrange
            var sql = "CREATE VIEW [dbo].[UserOrderSummary] AS SELECT u.UserName, COUNT(o.OrderId) AS OrderCount, SUM(o.Total) AS TotalAmount FROM Users u LEFT JOIN Orders o ON u.UserId = o.UserId GROUP BY u.UserName;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[UserOrderSummary]', N'V') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP VIEW [dbo].[UserOrderSummary];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains(sql));
        }
    }
}