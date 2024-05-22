using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Models.ViewModels;
using MyBookyWeb.Areas.Customer.Controllers;
using System.Linq.Expressions;
using System.Security.Claims;

namespace MyBookyWebUnitTest
{
    public class HomeControllerUnitTest
    {

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
            Assert.Equal(2, model.Products.Count);
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

        [Fact]
        public void Details_Returns_Correct_View_With_Cart_Model()
        {
            // Arrange
            var id = 1;
            var product = new Product { Id = id, Title = "Test Product", Category = new Category { Name = "TestCategory" } };
            var shoppingCart = new ShoppingCart { Product = product, ProductId = id, Count = 1 };

            var unitOfWorkMock = new Mock<IUnitofWork>();
            var logger = new Mock<ILogger<HomeController>>();
            unitOfWorkMock.Setup(u => u.Product.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), false)).Returns(product);

            var homeCtrl = new HomeController(logger.Object, unitOfWorkMock.Object);

            // Act
            var result = homeCtrl.Details(id) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Details", result.ViewName ?? string.Empty);
            Assert.IsType<ShoppingCart>(result.Model);

            var model = result.Model as ShoppingCart;
            Assert.Equal(shoppingCart.ProductId, model.ProductId);
        }
        [Fact]
        public void Details_Adds_Product_To_Cart_When_Not_Already_In_Cart()
        {
            // Arrange
            var shoppingCart = new ShoppingCart
            {
                ProductId = 1,
                Count = 1
            };

            var userId = "user123"; // Mock user ID

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            DefaultHttpContext httpContext = new()
            {
                Session = new TestSession(),
                User = claimsPrincipal
            };
            httpContext.Request.Host = new HostString("localhost");
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var unitOfWorkMock = new Mock<IUnitofWork>();
            var logger = new Mock<ILogger<HomeController>>();
            _ = unitOfWorkMock.Setup(u => u.ShoppingCart.Get(It.IsAny<Expression<Func<ShoppingCart, bool>>>(), It.IsAny<string>(), false))
                .Returns(value: shoppingCart); // Cart not found in database

            var controller = new HomeController(logger.Object, unitOfWorkMock.Object)
            {
                ControllerContext = controllerContext,
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
            // Act
            var result = controller.Details(shoppingCart) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Product successfully added to cart.", controller.TempData["success"]);
        }

    }

}