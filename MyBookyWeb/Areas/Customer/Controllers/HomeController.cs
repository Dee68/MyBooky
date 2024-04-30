using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Utilities;
using System.Diagnostics;
using System.Security.Claims;

namespace MyBookyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _unitofWork;
        public HomeController(ILogger<HomeController> logger, IUnitofWork unitofWork)
        {
            _logger = logger;
            _unitofWork = unitofWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _unitofWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(products);
        }
        [HttpGet]
        public IActionResult Index(string? query)
        {
            var productsEnumerable = _unitofWork.Product.GetAll(includeProperties: "Category,ProductImages");
            IQueryable<Product> products = productsEnumerable.AsQueryable();
            if (products == null || !products.Any())
            {
                return Problem("There are no books yet in the database");
            }
            
            if (!String.IsNullOrEmpty(query))
            {
                query = query.Trim().ToLower();
                products = products.Where(q => q.Title!.ToLower().Contains(query) 
                || q.Author!.ToLower().Contains(query) || q.ISBN!.Contains(query) 
                || q.Category!.Name!.ToLower().Contains(query) || q.Price.ToString().Contains(query));
            }
            var searchedProducts = products.ToList();
            return View(searchedProducts);
        }
        public IActionResult Details(int id)
        {
            ShoppingCart cart = new ShoppingCart()
            {
                Product = _unitofWork.Product.Get(u => u.Id==id,includeProperties: "Category,ProductImages"),
                ProductId = id,
                Count = 1
        };
            
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;//corrects identity_insert set to off error
            //retriev logged in user Id
           var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCart.ApplicationUserId = userId;
            
            //check if product already in cart
            ShoppingCart cartFromDb = _unitofWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
            u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                //product already in cart
                //get count of product in cart and update new count
                cartFromDb.Count += shoppingCart.Count;
                //update cart
                _unitofWork.ShoppingCart.Update(cartFromDb);
                _unitofWork.Save();


            }
            else
            {
                //product not in cart
                _unitofWork.ShoppingCart.Add(shoppingCart);
                _unitofWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
                
            }
           
            TempData["success"] = "Product successfully added to cart.";
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
