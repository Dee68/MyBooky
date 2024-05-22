using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Models.ViewModels;
using MyBooky.Utilities;
using Stripe.Checkout;
using System.Security.Claims;

namespace MyBookyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController(IUnitofWork unitofWork, IEmailSender emailSender, ILogger<CartController> logger) : Controller
    {
        private readonly IUnitofWork _unitofWork = unitofWork;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly ILogger<CartController> _logger = logger;
        [BindProperty]
        public ShoppingCartVm ShoppingCartVm { get; set; }

        public IActionResult Index()
        {
            //get the userId
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            // Initialize the ShoppingCartVm object
            ShoppingCartVm = new ShoppingCartVm()
            {
                ShoppingCartList = [], // Initialize an empty list
                OrderHead = new()
            };
            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;
                ShoppingCartVm = new ShoppingCartVm()
                {
                    ShoppingCartList = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                  includeProperties: "Product"),
                    OrderHead = new()
                };
                try
                {
                    IEnumerable<ProductImage> productImages = _unitofWork.ProductImage.GetAll();

                    foreach (var cart in ShoppingCartVm.ShoppingCartList)
                    {
                        cart.Product ??= new Product(); // Ensure Product is not null
                        cart.Product.ProductImages = productImages?.Where(u => u.ProductId == cart.Product.Id).ToList();
                        cart.Price = GetPriceBasedOnQuantity(cart);
                        ShoppingCartVm.OrderHead.OrderTotal += (cart.Price * cart.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while loading the productImage.");
                    foreach (var cart in ShoppingCartVm.ShoppingCartList)
                    {
                        cart.Product ??= new Product(); // Ensure Product is not null
                        cart.Product.ProductImages = [];
                    }
                }

            }




            return View(ShoppingCartVm);
        }
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitofWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitofWork.ShoppingCart.Update(cartFromDb);
            _unitofWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitofWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                //remove that from cart

                _unitofWork.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, _unitofWork.ShoppingCart
                    .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitofWork.ShoppingCart.Update(cartFromDb);
            }

            _unitofWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitofWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb != null)
            {
                _unitofWork.ShoppingCart.Remove(cartFromDb);
                if (HttpContext != null && HttpContext.Session != null)
                {
                    var cartCount = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count();
                    HttpContext.Session.SetInt32(SD.SessionCart, cartCount - 1);
                }
                _unitofWork.Save();
            }


            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            //get the userId
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // Redirect if user is not authenticated
                return RedirectToAction(nameof(Index));
            }

            var userId = userIdClaim.Value;
            // Initialize the ShoppingCartVm object
            ShoppingCartVm = new ShoppingCartVm()
            {
                ShoppingCartList = [], // Initialize an empty list
                OrderHead = new()
            };

            ShoppingCartVm.ShoppingCartList = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            if (!ShoppingCartVm.ShoppingCartList.Any())
            {
                TempData["error"] = "Your cart is empty you can not proceed";
                return RedirectToAction(nameof(Index));

            }
            try
            {
                ShoppingCartVm.OrderHead.ApplicationUser = _unitofWork.ApplicationUser.Get(u => u.Id == userId);

                ShoppingCartVm.OrderHead.Name = ShoppingCartVm.OrderHead.ApplicationUser.Name;
                ShoppingCartVm.OrderHead.StreetAddress = ShoppingCartVm.OrderHead.ApplicationUser.StreetAddress;
                ShoppingCartVm.OrderHead.City = ShoppingCartVm.OrderHead.ApplicationUser.City;
                ShoppingCartVm.OrderHead.State = ShoppingCartVm.OrderHead.ApplicationUser.State;
                ShoppingCartVm.OrderHead.PostalCode = ShoppingCartVm.OrderHead.ApplicationUser.PostalCode;
                //for each cartitem get their order total
                foreach (var cart in ShoppingCartVm.ShoppingCartList)
                {
                    cart.Price = GetPriceBasedOnQuantity(cart);
                    ShoppingCartVm.OrderHead.OrderTotal += (cart.Price * cart.Count);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the cart.");
                return View(ShoppingCartVm);

            }
            return View(ShoppingCartVm);



        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            //get the userId
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // Redirect if user is not authenticated
                return RedirectToAction(nameof(Index));
            }
            var userId = userIdClaim.Value;

            // Check if ShoppingCartVm and OrderHead are initialized
            if (ShoppingCartVm != null && ShoppingCartVm.OrderHead != null)
            {

                ShoppingCartVm.ShoppingCartList = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

                ShoppingCartVm.OrderHead.OrderDate = System.DateTime.Now;
                ShoppingCartVm.OrderHead.ApplicationUserId = userId;

                ApplicationUser applicationUser = _unitofWork.ApplicationUser.Get(u => u.Id == userId);


                //for each cartitem get their order total
                foreach (var cart in ShoppingCartVm.ShoppingCartList)
                {
                    cart.Price = GetPriceBasedOnQuantity(cart);
                    ShoppingCartVm.OrderHead.OrderTotal += (cart.Price * cart.Count);
                }

                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    //it is a regular customer 
                    ShoppingCartVm.OrderHead.PaymentStatus = SD.PaymentStatusPending;
                    ShoppingCartVm.OrderHead.OrderStatus = SD.StatusPending;
                }
                else
                {
                    //it is a company user
                    ShoppingCartVm.OrderHead.PaymentStatus = SD.PaymentStatusDelayedPayment;
                    ShoppingCartVm.OrderHead.OrderStatus = SD.StatusApproved;
                }
                _unitofWork.OrderHead.Add(ShoppingCartVm.OrderHead);
                _unitofWork.Save();
                foreach (var cart in ShoppingCartVm.ShoppingCartList)
                {
                    OrderDetail orderDetail = new()
                    {
                        ProductId = cart.ProductId,
                        OrderHeadId = ShoppingCartVm.OrderHead.Id,
                        Price = cart.Price,
                        Count = cart.Count
                    };
                    _unitofWork.OrderDetail.Add(orderDetail);
                    _unitofWork.Save();
                }
                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    //it is a regular customer account and we need to capture payment
                    //stripe logic
                    var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                    var options = new SessionCreateOptions
                    {
                        SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVm.OrderHead.Id}",
                        CancelUrl = domain + $"Customer/Cart/index",
                        LineItems = [],
                        Mode = "payment"
                    };
                    foreach (var item in ShoppingCartVm.ShoppingCartList)
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100),
                                Currency = "eur",
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
                    _unitofWork.OrderHead.UpdateStripePaymentID(ShoppingCartVm.OrderHead.Id, session.Id, session.PaymentIntentId);
                    _unitofWork.Save();
                    Response.Headers.Append("Location", session.Url);
                    return new StatusCodeResult(303);

                }

                return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVm.OrderHead.Id });
            }
            else
            {

                // Output the error to the console
                Console.WriteLine($"An error occurred:");
                return RedirectToAction("Index", "Home");

            }



        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHead orderHead = _unitofWork.OrderHead.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHead.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by a customer
                var service = new SessionService();
                Session session = service.Get(orderHead.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofWork.OrderHead.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitofWork.OrderHead.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitofWork.Save();
                }
                HttpContext.Session.Clear();

            }

            var orderDetails = _unitofWork.OrderDetail.GetAll(u => u.OrderHeadId == id, includeProperties: "Product").ToList();

            //Build order detail string
            string orderDetailString = "<ul>";
            foreach (var item in orderDetails)
            {
                orderDetailString += $"<li>Product: {item.Product.Title}, Quantity: {item.Count}, Price: {item.Price:C}</li>";
            }
            orderDetailString += "</ul>";

            string emailBody = $@"
                <p>New Order Created - {orderHead.Id}</p>
                <p>Order Details:</p>
                {orderDetailString}
            ";

            _emailSender.SendEmailAsync(orderHead.ApplicationUser.Email, "New Order - MyBooky", emailBody);

            List<ShoppingCart> shoppingCarts = _unitofWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == orderHead.ApplicationUserId).ToList();

            _unitofWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitofWork.Save();

            return View(id);
        }
        private static double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}