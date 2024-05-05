using MyBooky.Models;
using Moq;
using MyBooky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Hosting;
using MyBooky.Models.ViewModels;
using MyBooky.DataAccess.Repository;
using Castle.Core.Logging;
using MyBookyWeb.Areas.Customer.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MyBookyWebUnitTest
{
    public class HomeControllerUnitTest
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
                ProductImages = new List<ProductImage> { }
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
                ProductImages = new List<ProductImage> { }
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
                ProductImages = new List<ProductImage> { }
            };
            var productList = new List<Product> { p1, p2, p3 };

            var _unitofwork = new Mock<IUnitofWork>();
            var logger = new Mock<ILogger<HomeController>>();
           
            _unitofwork.Setup(u => u.Product.GetAll(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>())).Returns(productList);

            var homeCtrl = new HomeController(logger.Object,_unitofwork.Object);
            //Action
            var results = homeCtrl.Index();
            // Assert
            Assert.NotNull(results);
            Assert.IsType<ViewResult>(results);

            var viewResult = (ViewResult)results;
            Assert.NotNull(viewResult.Model);
            Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);

            var model = (IEnumerable<Product>)viewResult.Model;
            Assert.Equal(3, model.Count());
        }

        [Fact]
        public void Index_Returns_View_With_Paginated_Products()
        {
            // Arrange
            var p1 = new Product { Id = 1, Title = "Book1", Author = "Jon Doe", ISBN = "121212121" };
            var p2 = new Product { Id = 2, Title = "Book2", Author = "Jane Long", ISBN = "131212121" };
            var p3 = new Product { Id = 3, Title = "Book3", Author = "Jack Neils", ISBN = "141212141" };
            var productList = new List<Product> { p1, p2, p3 };

            var _unitofwork = new Mock<IUnitofWork>();
            var logger = new Mock<ILogger<HomeController>>();

            _unitofwork.Setup(u => u.Product.GetAll(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>())).Returns(productList);

            var homeCtrl = new HomeController(logger.Object, _unitofwork.Object);

            // Action
            var result = homeCtrl.Index(query: null, page: 1, pageSize: 2) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var model = Assert.IsAssignableFrom<ProductListVm>(result.Model);
            Assert.Equal(2, model.Products.Count());
            Assert.Equal(2, model.TotalPages);
        }

        [Fact]
        public void Index_Returns_Problem_When_No_Products_Found()
        {
            // Arrange
            var emptyProductList = new List<Product>();

            var _unitofwork = new Mock<IUnitofWork>();
            var logger = new Mock<ILogger<HomeController>>();

            _unitofwork.Setup(u => u.Product.GetAll(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>())).Returns(emptyProductList);

            var homeCtrl = new HomeController(logger.Object, _unitofwork.Object);

            // Action
            var result = homeCtrl.Index(query: null, page: 1, pageSize: 2) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, result.StatusCode);
            Assert.IsType<ProblemDetails>(result.Value);
            var problemDetails = (ProblemDetails)result.Value;
            Assert.Equal("There are no books yet in the database", problemDetails.Detail);
        }

    }
}
