using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Models.ViewModels;
using Stripe;
using Stripe.Checkout;
using MyBooky.Utilities;
using System.Diagnostics;
using System.Security.Claims;
using MyBooky.DataAccess.Repository;
using Stripe.Climate;

namespace MyBookyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private IUnitofWork _unitofWork;
        [BindProperty]
        public OrderVm OrderVm { get; set; }
        public OrderController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult Index()
        {
            return View("Index");
        }
        public IActionResult Details(int orderId)
        {
            OrderVm orderVm = new OrderVm()
            {
                OrderHead = _unitofWork.OrderHead.Get(u =>u.Id == orderId, includeProperties:"ApplicationUser"),
                OrderDetail = _unitofWork.OrderDetail.GetAll(u =>u.OrderHeadId == orderId, includeProperties:"Product")
            };
            return View(orderVm);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            OrderHead orderHeadFromDb = _unitofWork.OrderHead.Get(u => u.Id == OrderVm.OrderHead.Id);
            orderHeadFromDb.Name = OrderVm.OrderHead.Name;
            orderHeadFromDb.StreetAddress = OrderVm.OrderHead.StreetAddress;
            orderHeadFromDb.City = OrderVm.OrderHead.City;
            orderHeadFromDb.State = OrderVm.OrderHead.State;
            orderHeadFromDb.PostalCode = OrderVm.OrderHead.PostalCode;
            if (!string.IsNullOrEmpty(OrderVm.OrderHead.Carrier))
            {
                orderHeadFromDb.Carrier = OrderVm.OrderHead.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVm.OrderHead.TrackingNumber))
            {
                orderHeadFromDb.Carrier = OrderVm.OrderHead.TrackingNumber;
            }
            _unitofWork.OrderHead.Update(orderHeadFromDb);
            _unitofWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new { orderId = orderHeadFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            if(OrderVm.OrderHead == null || OrderVm.OrderHead == null) 
            {
                return Problem("Bad request", null, 404);
            }
            _unitofWork.OrderHead.UpdateStatus(OrderVm.OrderHead.Id, SD.StatusInProcess);
            _unitofWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHead.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHead = _unitofWork.OrderHead.Get(u => u.Id == OrderVm.OrderHead.Id);
            orderHead.TrackingNumber = OrderVm.OrderHead.TrackingNumber;
            orderHead.Carrier = OrderVm.OrderHead.Carrier;
            orderHead.OrderStatus = SD.StatusShipped;
            orderHead.ShippingDate = DateTime.Now;
            if (orderHead.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHead.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitofWork.OrderHead.Update(orderHead);
            _unitofWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHead.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {

            var orderHead = _unitofWork.OrderHead.Get(u => u.Id == OrderVm.OrderHead.Id);

            if (orderHead.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHead.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitofWork.OrderHead.UpdateStatus(orderHead.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitofWork.OrderHead.UpdateStatus(orderHead.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitofWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHead.Id });

        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVm.OrderHead = _unitofWork.OrderHead
                .Get(u => u.Id == OrderVm.OrderHead.Id, includeProperties: "ApplicationUser");
            OrderVm.OrderDetail = _unitofWork.OrderDetail
                .GetAll(u => u.OrderHeadId == OrderVm.OrderHead.Id, includeProperties: "Product");

            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVm.OrderHead.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVm.OrderHead.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVm.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            _unitofWork.OrderHead.UpdateStripePaymentID(OrderVm.OrderHead.Id, session.Id, session.PaymentIntentId);
            _unitofWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeadId)
        {

            OrderHead orderHead = _unitofWork.OrderHead.Get(u => u.Id == orderHeadId);
            if (orderHead.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHead.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofWork.OrderHead.UpdateStripePaymentID(orderHeadId, session.Id, session.PaymentIntentId);
                    _unitofWork.OrderHead.UpdateStatus(orderHeadId, orderHead.OrderStatus, SD.PaymentStatusApproved);
                    _unitofWork.Save();
                }


            }


            return View(orderHeadId);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHead> objOrderHeads;


            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeads = _unitofWork.OrderHead.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeads = _unitofWork.OrderHead
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }


            switch (status)
            {
                case "pending":
                    objOrderHeads = objOrderHeads.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeads = objOrderHeads.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeads = objOrderHeads.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeads = objOrderHeads.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;

            }


            return Json(new { data = objOrderHeads });
        }


        #endregion
}
}
