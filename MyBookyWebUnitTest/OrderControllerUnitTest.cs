using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Models.ViewModels;
using MyBooky.Utilities;
using MyBookyWeb.Areas.Admin.Controllers;
using System.Linq.Expressions;

namespace MyBookyWebUnitTest
{
    public class OrderControllerUnitTest
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var loggerMock = new Mock<ILogger<OrderController>>();
            var controller = new OrderController(mockUnitOfWork.Object, loggerMock.Object);

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
            var loggerMock = new Mock<ILogger<OrderController>>();
            var orderId = 1;

            // Mock OrderHead and OrderDetail data
            var orderHead = new OrderHead { Id = orderId };
            var orderDetail = new List<OrderDetail>
        {
            new() { OrderHeadId = orderId, Product = new Product { Title = "Product 1" } },
            new() { OrderHeadId = orderId, Product = new Product { Title = "Product 2" } }
        };
            mockUnitOfWork.Setup(uow => uow.OrderHead.Get(It.IsAny<Expression<Func<OrderHead, bool>>>(), It.IsAny<string>(), false)).Returns(orderHead);
            mockUnitOfWork.Setup(uow => uow.OrderDetail.GetAll(It.IsAny<Expression<Func<OrderDetail, bool>>>(), It.IsAny<string>())).Returns(orderDetail);

            var controller = new OrderController(mockUnitOfWork.Object, loggerMock.Object);

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
        public void StartProcessing_ValidOrderVm_UpdatesOrderStatus()
        {
            // Arrange
            var orderHeadId = 1;
            var orderHead = new OrderHead { Id = orderHeadId };
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var loggerMock = new Mock<ILogger<OrderController>>();
            var _controller = new OrderController(mockUnitOfWork.Object, loggerMock.Object)
            {
                OrderVm = new OrderVm { OrderHead = orderHead }
            };
            mockUnitOfWork.Setup(uow => uow.OrderHead.UpdateStatus(orderHeadId, SD.StatusInProcess, SD.PaymentStatusApproved));
            mockUnitOfWork.Setup(uow => uow.Save());
            //Setup
            // Mock TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData; // Set TempData in the controller
            // Act
            var result = _controller.StartProcessing();

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectToAction = (RedirectToActionResult)result;
            Assert.Equal("Details", redirectToAction.ActionName);
            Assert.Equal(orderHeadId, redirectToAction.RouteValues["orderId"]);
            Assert.Equal("Order Details Updated Successfully.", _controller.TempData["Success"]);
        }

    }
}