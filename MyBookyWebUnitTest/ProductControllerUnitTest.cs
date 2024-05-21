using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MyBooky.DataAccess.Repository;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Models.ViewModels;
using MyBookyWeb.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;


namespace MyBookyWebUnitTest
{
    public class ProductControllerUnitTest
    {
        [Fact]
        public void Index()
        {
            //Arrangement

            var p1 = new Product
            {
                Id = 1,
                Title = "Book1",
                CategoryId = 1,
                ISBN = "121212121",
                ListPrice = 30.0,
                Price50 = 27.0,
                Price100 = 25.0,
                Author = "Jon Doe",
                Description = "Description1",
                ProductImages = []
            };
            var p2 = new Product
            {
                Id = 2,
                Title = "Book2",
                CategoryId = 1,
                ISBN = "131212121",
                ListPrice = 30.0,
                Price50 = 27.0,
                Price100 = 25.0,
                Author = "Jane Long",
                Description = "Description1",
                ProductImages = []
            };
            var p3 = new Product
            {
                Id = 3,
                Title = "Book3",
                CategoryId = 2,
                ISBN = "141212141",
                ListPrice = 30.0,
                Price50 = 27.0,
                Price100 = 25.0,
                Author = "Jack Neils",
                Description = "Description1",
                ProductImages = []
            };
            var productList = new List<Product> { p1, p2, p3 };

            var _unitofwork = new Mock<IUnitofWork>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _unitofwork.Setup(u => u.Product.GetAll(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>())).Returns(productList);

            var productCtrl = new ProductController(_unitofwork.Object, webHostEnvironmentMock.Object);

            //Action
            var results = productCtrl.Index() as ViewResult;

            //Assertion
            Assert.NotNull(results);
            Assert.IsType<List<Product>>(results.Model);
            var model = results.Model as List<Product>;
            Assert.Equal(productList.Count, model.Count);
        }

        [Fact]
        public void Upsert_ReturnsViewResult_WhenIdIsNull()
        {
            //Arrangement
            var _unitofwork = new Mock<IUnitofWork>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            var expectedCategoryList = new List<Category>
            {
                 new() { Id = 1, Name = "Category1" },
                 new() { Id = 2, Name = "Category2" }
            };

            _unitofwork.Setup(u => u.Category.GetAll()).Returns(expectedCategoryList);

            var productCtrl = new ProductController(_unitofwork.Object, webHostEnvironmentMock.Object);
            //Action
            var result = productCtrl.Upsert(null) as ViewResult;
            //Assertion
            Assert.NotNull(result);
            var model = result.Model as ProductVm;
            Assert.NotNull(model);
            Assert.NotNull(model.Product);
        }

        [Fact]
        public void Upsert_ReturnsViewResult_WhenIdIsZero()
        {
            // Arrange
            var _unitofWork = new Mock<IUnitofWork>();
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            var expectedCategoryList = new List<Category>
            {
                 new() { Id = 1, Name = "Category1" },
                 new() { Id = 2, Name = "Category2" }
            };
            _unitofWork.Setup(u => u.Category.GetAll()).Returns(expectedCategoryList);
            var productCtrl = new ProductController(_unitofWork.Object, webHostEnvironmentMock.Object);

            // Act
            var result = productCtrl.Upsert(0) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as ProductVm;
            Assert.NotNull(model);
            Assert.NotNull(model.Product);
        }



    }

}