using MyBooky.Models;
using Moq;
using MyBooky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Hosting;
using MyBooky.Models.ViewModels;
using MyBooky.DataAccess.Repository;
using Castle.Core.Logging;
using MyBookyWeb.Areas.Customer.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using MyBooky.Utilities;
using System.Security.Claims;
using MyBookyWeb.Areas.Admin.Controllers;

namespace MyBookyWebUnitTest
{
    public class OrderControllerUnitTest
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Model);
            Assert.Equal("Index", result.ViewName);
        }
        [Fact]
        public void Details_ReturnsViewResultWithCorrectModel()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var orderVm = new OrderVm();
            var orderId = 1;

            // Mock OrderHead and OrderDetail data
            var orderHead = new OrderHead { Id = orderId };
            var orderDetail = new List<OrderDetail>
        {
            new OrderDetail { OrderHeadId = orderId, Product = new Product { Title = "Product 1" } },
            new OrderDetail { OrderHeadId = orderId, Product = new Product { Title = "Product 2" } }
        };
            mockUnitOfWork.Setup(uow => uow.OrderHead.Get(It.IsAny<Expression<Func<OrderHead, bool>>>(), It.IsAny<string>(), false)).Returns(orderHead);
            mockUnitOfWork.Setup(uow => uow.OrderDetail.GetAll(It.IsAny<Expression<Func<OrderDetail, bool>>>(), It.IsAny<string>())).Returns(orderDetail);

            var controller = new OrderController(mockUnitOfWork.Object);

            // Act
            var result = controller.Details(orderId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OrderVm>(result.Model);
            var model = result.Model as OrderVm;
            Assert.Equal(orderHead, model.OrderHead);
            Assert.Equal(orderDetail, model.OrderDetail);
        }
        [Fact]
        public void StartProcessing_ReturnsErrorWithNull_OrderVm()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var controller = new OrderController(mockUnitOfWork.Object);
            controller.OrderVm = new OrderVm { OrderHead = null }; // Simulating OrderHead as null
            // Act
            var result = controller.StartProcessing() as ObjectResult;

            // Assert that the value is a ProblemDetails object
            var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
            // Assert specific properties of ProblemDetails
            Assert.Equal("Bad request", problemDetails.Detail);
            Assert.Null(problemDetails.Title);
            Assert.Null(problemDetails.Instance);

            // Assert
            //Assert.NotNull(result);
            //Assert.Equal("Details", result.ActionName);
            //Assert.Equal("Admin", result.RouteValues["area"]); // Assuming "Admin" is the area
            //Assert.Equal(orderId, result.RouteValues["orderId"]);

            // Assert that the order status is updated to "In Process"
            //mockUnitOfWork.Verify(uow => uow.OrderHead.UpdateStatus(orderId, SD.StatusInProcess), Times.Once);

            // Assert that changes are saved
           // mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);

            // Assert that a success message is set in TempData
           // Assert.Equal("Order Details Updated Successfully.", controller.TempData["Success"]);
        }
        //[Fact]
        //public void StartProcessing_ReturnsRedirectToDetailsWithCorrectOrderId()
        //{
        //    // Arrange
        //    var mockUnitOfWork = new Mock<IUnitofWork>();
        //    var controller = new OrderController(mockUnitOfWork.Object);
        //    //ApplicationUser
        //    var applicationUser = new ApplicationUser
        //    {
        //        Id = "userId123",
        //        UserName = "testuser@example.com",
        //    };
        //    var orderId = 1;
        //    var orderHead = new OrderHead
        //    {
        //        Id = orderId,
        //        Name = "John Doe",
        //        StreetAddress = "123 Main St",
        //        City = "Anytown",
        //        State = "NY",
        //        PostalCode = "12345",
        //        ApplicationUser = applicationUser
        //    };
        //    // Assign the OrderHead to OrderVm
        //    var orderVm = new OrderVm
        //    {
        //        OrderHead = orderHead
        //    };

        //    controller.OrderVm = orderVm;

        //    //Act
        //    var result = controller.StartProcessing() as RedirectToActionResult;

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal("Details", result.ActionName);
        //    Assert.Equal("Admin", result.RouteValues["area"]); // Assuming "Admin" is the area
        //    Assert.Equal(orderId, result.RouteValues["orderId"]);


        //}


    }
}
