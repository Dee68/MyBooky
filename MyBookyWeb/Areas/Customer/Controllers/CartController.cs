using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Models.ViewModels;
using MyBooky.Utilities;
using System.Security.Claims;

namespace MyBookyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        public ShoppingCartVm ShoppingCartVm { get; set; }
        public CartController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult Index()
        {
            //get the userId
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVm = new ShoppingCartVm()
            {
                ShoppingCartList = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHead = new ()

            };
            //for each cartitem get their order total
            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVm.OrderHead.OrderTotal += (cart.Price * cart.Count);
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

            _unitofWork.ShoppingCart.Remove(cartFromDb);

            HttpContext.Session.SetInt32(SD.SessionCart, _unitofWork.ShoppingCart
              .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitofWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
			//get the userId
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVm = new ShoppingCartVm()
			{
				ShoppingCartList = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
				includeProperties: "Product"),
				OrderHead = new()

			};
            ShoppingCartVm.OrderHead.ApplicationUser = _unitofWork.ApplicationUser.Get(u => u.Id == userId);

			ShoppingCartVm.OrderHead.Name = ShoppingCartVm.OrderHead.ApplicationUser.Name;
			ShoppingCartVm.OrderHead.StreetAddress = ShoppingCartVm.OrderHead.ApplicationUser.StreetAddress;
			ShoppingCartVm.OrderHead.City = ShoppingCartVm. OrderHead.ApplicationUser.City;
			ShoppingCartVm.OrderHead.State = ShoppingCartVm.OrderHead.ApplicationUser.State;
			ShoppingCartVm.OrderHead.PostalCode = ShoppingCartVm.OrderHead.ApplicationUser.PostalCode;

			//for each cartitem get their order total
			foreach (var cart in ShoppingCartVm.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVm.OrderHead.OrderTotal += (cart.Price * cart.Count);
			}

			return View(ShoppingCartVm);
        }
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
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
