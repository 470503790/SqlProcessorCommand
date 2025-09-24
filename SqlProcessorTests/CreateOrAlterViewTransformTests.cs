using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlProcessorCommand;

namespace SqlProcessorTests
{
    [TestClass]
    public class CreateOrAlterViewTransformTests
    {
        private CreateOrAlterViewTransform _transform;

        [TestInitialize]
        public void Setup()
        {
            _transform = new CreateOrAlterViewTransform();
        }

        [TestMethod]
        public void CanHandle_ValidCreateOrAlterView_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE OR ALTER VIEW [TestSchema].[TestView] AS SELECT * FROM TestTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateOrAlterViewWithoutSchema_ReturnsTrue()
        {
            // Arrange
            var sql = "CREATE OR ALTER VIEW TestView AS SELECT ID, Name FROM Users;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanHandle_CreateViewOnly_ReturnsFalse()
        {
            // Arrange
            var sql = "CREATE VIEW [TestSchema].[TestView] AS SELECT * FROM TestTable;";

            // Act
            var result = _transform.CanHandle(sql);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Transform_CreateOrAlterViewWithSchema_ConvertsToCompatibleForm()
        {
            // Arrange
            var sql = "CREATE OR ALTER VIEW [TestSchema].[TestView] AS SELECT * FROM TestTable;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[TestSchema].[TestView]', N'V') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP VIEW [TestSchema].[TestView];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE VIEW [TestSchema].[TestView] AS SELECT * FROM TestTable;"));
        }

        [TestMethod]
        public void Transform_CreateOrAlterViewWithoutSchema_UsesDefaultSchema()
        {
            // Arrange
            var sql = "CREATE OR ALTER VIEW TestView AS SELECT ID, Name FROM Users;";

            // Act
            var result = _transform.Transform(sql);

            // Assert
            Assert.IsTrue(result.Contains("IF OBJECT_ID(N'[dbo].[TestView]', N'V') IS NOT NULL"));
            Assert.IsTrue(result.Contains("DROP VIEW [dbo].[TestView];"));
            Assert.IsTrue(result.Contains("GO"));
            Assert.IsTrue(result.Contains("CREATE VIEW TestView AS SELECT ID, Name FROM Users;"));
        }
    }
}