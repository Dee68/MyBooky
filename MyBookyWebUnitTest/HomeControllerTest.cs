using Microsoft.AspNetCore.Mvc;
using Moq;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBookyWeb.Areas.Customer.Controllers;
using MyBooky.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;

namespace MyBookyWebUnitTest
{
    //Integration test for home controller
    public class HomeControllerTest
    {
        [Fact]
        public void Index_ReturnsViewWithProducts()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HomeController>>();
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var products = new List<Product>
            {
                new Product { 
                    Id = 1, 
                    Title = "Book 1", 
                    CategoryId = 1,
                    Category = new Category { Id = 1, Name = "TestCategory", DisplayOrder = 1 },
                    Description = "Book one", 
                    ISBN = "1212121212", 
                    ProductImages = new List<ProductImage>(), 
                    Price = 20.0, ListPrice = 20.0, 
                    Price50 = 18.0, 
                    Price100 = 15.0, 
                    Author = "John Doe" },
                new Product { 
                    Id = 2, 
                    Title = "Book 2", 
                    CategoryId = 2, 
                    Category = new Category { Id = 2, Name = "TestCategory1", DisplayOrder= 2 },
                    Description = "Book two",
                    ISBN = "1313121219",
                    ProductImages = new List<ProductImage>(),
                    Price = 20.0, ListPrice = 20.0,
                    Price50 = 18.0,
                    Price100 = 15.0,
                    Author = "Jane Doe"}
            };
            mockUnitOfWork.Setup(uow => uow.Product.GetAll(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>())).Returns(products);
            var controller = new HomeController(mockLogger.Object, mockUnitOfWork.Object);
            //Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as IEnumerable<Product>;
            Assert.NotNull(model);
            Assert.Equal(2, model.Count());
        }
        [Fact]
        public void Details_ReturnsViewWithProduct()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HomeController>>();
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var productId = 1;
            var product = new Product
            {
                Id = 1,
                Title = "Book 1",
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "TestCategory", DisplayOrder = 1 },
                Description = "Book one",
                ISBN = "1212121212",
                ProductImages = new List<ProductImage>(),
                Price = 20.0,
                ListPrice = 20.0,
                Price50 = 18.0,
                Price100 = 15.0,
                Author = "John Doe"
            };
            var shoppingCart = new ShoppingCart { Product = product, ProductId = productId, Count = 1 };
            mockUnitOfWork.Setup(uow => uow.Product.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), false)).Returns(product);
            var controller = new HomeController(mockLogger.Object, mockUnitOfWork.Object);

            // Act
            var result = controller.Details(productId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ViewName ?? string.Empty);
            Assert.IsType<ShoppingCart>(result.Model);

            var model = result.Model as ShoppingCart;
            Assert.Equal(shoppingCart.ProductId, model.ProductId);
        }
        [Fact]
        public void Details_RedirectsToLoginForUnauthenticatedUserWhenAddingToCart()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HomeController>>();
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var controller = new HomeController(mockLogger.Object, mockUnitOfWork.Object);

            // Mock HttpContext with unauthenticated user
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            controller.ControllerContext = controllerContext;

            // Act
            var result = controller.Details(new ShoppingCart()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

    }
}
