using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBookyWeb.Areas.Customer.Controllers;
using System.Linq.Expressions;

namespace MyBookyWebUnitTest
{
  //Integration test for home controller
  public class HomeControllerTest
  {
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