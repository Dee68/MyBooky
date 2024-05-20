using Castle.Core.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MyBooky.DataAccess.Repository;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Models.ViewModels;
using MyBookyWeb.Areas.Customer.Controllers;
using Stripe.BillingPortal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Claims;
using Xunit;

namespace MyBookyWebUnitTest
{
    public class CartControllerUnitTest
    {
        [Fact]
        public void Index_Returns_Correct_View_With_ShoppingCartVm()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitofWork>();
            var emailSenderMock = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();


            var userId = "user123";
            var shoppingCartList = new List<ShoppingCart>();
            for (int i = 1; i <= 3; i++)
            {
                var shoppingCart = new ShoppingCart
                {
                    Id = i,
                    ProductId = i,
                    Product = new Product
                    {
                        /* Populate Product properties */
                        Id = i,
                        Title = "Book" + i,
                        CategoryId = 1,
                        ISBN = "121212121",
                        ListPrice = 30.0,
                        Price50 = 27.0,
                        Price100 = 25.0,
                        Author = "Jon Doe",
                        Description = "Description1",
                        ProductImages = new List<ProductImage> { }
                    },
                    Count = 1,
                    ApplicationUserId = "user123",
                    ApplicationUser = new ApplicationUser
                    {
                        /* Populate ApplicationUser properties */
                        Name = "John doe"
                    },
                    Price = 10.00 // Mock price
                };

                shoppingCartList.Add(shoppingCart);
            }


            var orderHead = new OrderHead
            {
                // Populate with mock order head data
                Id = 1,
                ApplicationUserId = "user123",
                ApplicationUser = new ApplicationUser { /* Populate ApplicationUser properties */ },
                OrderDate = DateTime.Now,
                ShippingDate = DateTime.Now.AddDays(1),
                OrderTotal = 100.00,
                OrderStatus = "Pending",
                PaymentStatus = "Pending",
                TrackingNumber = "1234567890",
                Carrier = "Carrier Name",
                PaymentDate = DateTime.Now,
                PaymentDueDate = DateTime.Now.AddDays(7),
                SessionId = "session123",
                PaymentIntentId = "paymentIntent123",
                Name = "John Doe",
                StreetAddress = "123 Main St",
                City = "Anytown",
                State = "NY",
                PostalCode = "12345"
            };

            unitOfWorkMock.Setup(u => u.ShoppingCart.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<ShoppingCart, bool>>>(), It.IsAny<string>()))
                .Returns(shoppingCartList);

            unitOfWorkMock.Setup(u => u.OrderHead.Add(It.IsAny<OrderHead>()));

            var controller = new CartController(unitOfWorkMock.Object, emailSenderMock.Object);

            var claimsIdentity = new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId) });
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<ShoppingCartVm>(result.Model);

        }
        [Fact]
        public void Plus_Method_Increments_Cart_Count_And_Redirects_To_Index()
        {
            // Arrange
            int cartId = 1;
            var cartFromDb = new ShoppingCart { Id = cartId, Count = 1 };
            var unitOfWorkMock = new Mock<IUnitofWork>();
            var emailSenderMock = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
            unitOfWorkMock.Setup(u => u.ShoppingCart.Get(It.IsAny<Expression<Func<ShoppingCart, bool>>>(), It.IsAny<string>(), false)).Returns(cartFromDb);
            var controller = new CartController(unitOfWorkMock.Object, emailSenderMock.Object);

            // Act
            var result = controller.Plus(cartId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(2, cartFromDb.Count); // Check if count is incremented
            unitOfWorkMock.Verify(u => u.ShoppingCart.Update(cartFromDb), Times.Once); // Verify Update method is called
            unitOfWorkMock.Verify(u => u.Save(), Times.Once); // Verify Save method is called
        }
        [Fact]
        public void Minus_Method_Decrease_Cart_Count_And_Redirects_TO_Index()
        {
            // Arrange
            int cartId = 1;
            var cartFromDb = new ShoppingCart { Id = cartId, Count = 3 };
            var unitOfWorkMock = new Mock<IUnitofWork>();
            var emailSenderMock = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
            unitOfWorkMock.Setup(u => u.ShoppingCart.Get(It.IsAny<Expression<Func<ShoppingCart,
                bool>>>(),
                It.IsAny<string>(),
                false))
                .Returns(cartFromDb);
            var controller = new CartController(unitOfWorkMock.Object, emailSenderMock.Object);
            //Action
            var result = controller.Minus(cartId) as RedirectToActionResult;
            //Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(2, cartFromDb.Count); // Check if count is decreased
        }
        [Fact]
        public void Remove_Method_Remove_From_Cart_And_Redirects_To_Index()
        {
            //Arrange
            int cartId = 1;
            var cartFromDb = new ShoppingCart { Id = cartId, Count = 3 };
            var unitOfWorkMock = new Mock<IUnitofWork>();
            var emailSenderMock = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
            unitOfWorkMock.Setup(u => u.ShoppingCart.Get(It.IsAny<Expression<Func<ShoppingCart,
                bool>>>(),
                It.IsAny<string>(),
                false))
                .Returns(cartFromDb);
            var controller = new CartController(unitOfWorkMock.Object, emailSenderMock.Object);

            //Action
            var result = controller.Remove(cartId);

            //Assert
            Assert.NotNull(result);
        }
        [Fact]
        public void Summary_Returns_Error_When_Cart_Is_Empty()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user123") });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var unitOfWorkMock = new Mock<IUnitofWork>();
            var emailSenderMock = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();


            var controller = new CartController(unitOfWorkMock.Object, emailSenderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Simulate empty shopping cart
            unitOfWorkMock.Setup(u => u.ShoppingCart.GetAll(It.IsAny<Expression<Func<ShoppingCart,
                bool>>>(),
                It.IsAny<string>()))
                .Returns<Expression<Func<ShoppingCart, bool>>, string>((predicate, includeProperties) =>
                {
                    // Check if the predicate filters by ApplicationUserId
                    if (predicate != null)
                    {
                        var userIdPredicate = (Expression<Func<ShoppingCart, bool>>)predicate;
                        // Simulate empty shopping cart for the user with the given ApplicationUserId
                        if (userIdPredicate.Compile()(new ShoppingCart { ApplicationUserId = "user123" }))
                        {
                            return new List<ShoppingCart>();
                        }
                    }
                    // If the predicate is null or doesn't match the expected condition, return null or an empty list
                    return null; // or new List<ShoppingCart>()
                });

            // Action
            var result = controller.Summary();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Your cart is empty you can not proceed", controller.TempData["error"]);
        }

        [Fact]
        public void Summary_Returns_Summary_View_With_Data()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user123") });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var unitOfWorkMock = new Mock<IUnitofWork>();
            var emailSenderMock = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
            var controller = new CartController(unitOfWorkMock.Object, emailSenderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Simulate non-empty shopping cart
            var shoppingCartList = new List<ShoppingCart>();
            for (int i = 1; i <= 3; i++)
            {
                var shoppingCart = new ShoppingCart
                {
                    Id = i,
                    ProductId = i,
                    Product = new Product
                    {
                        /* Populate Product properties */
                        Id = i,
                        Title = "Book" + i,
                        CategoryId = 1,
                        ISBN = "121212121",
                        ListPrice = 30.0,
                        Price50 = 27.0,
                        Price100 = 25.0,
                        Author = "Jon Doe",
                        Description = "Description1",
                        ProductImages = new List<ProductImage> { }
                    },
                    Count = 1,
                    ApplicationUserId = "user123",
                    ApplicationUser = new ApplicationUser
                    {
                        /* Populate ApplicationUser properties */
                        Name = "John doe"
                    },
                    Price = 10.00 // Mock price
                };

                shoppingCartList.Add(shoppingCart);
            }
            var orderHead = new OrderHead
            {
                // Populate with mock order head data
                Id = 1,
                ApplicationUserId = "user123",
                ApplicationUser = new ApplicationUser { /* Populate ApplicationUser properties */ },
                OrderDate = DateTime.Now,
                ShippingDate = DateTime.Now.AddDays(1),
                OrderTotal = 100.00,
                OrderStatus = "Pending",
                PaymentStatus = "Pending",
                TrackingNumber = "1234567890",
                Carrier = "Carrier Name",
                PaymentDate = DateTime.Now,
                PaymentDueDate = DateTime.Now.AddDays(7),
                SessionId = "session123",
                PaymentIntentId = "paymentIntent123",
                Name = "John Doe",
                StreetAddress = "123 Main St",
                City = "Anytown",
                State = "NY",
                PostalCode = "12345"
            };

            unitOfWorkMock.Setup(u => u.ShoppingCart.GetAll(It.IsAny<Expression<Func<ShoppingCart, bool>>>(), It.IsAny<string>()))
                .Returns(shoppingCartList);

            // Action
            var result = controller.Summary() as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<ShoppingCartVm>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(shoppingCartList.Count, model.ShoppingCartList.Count());
        }




    }
}
