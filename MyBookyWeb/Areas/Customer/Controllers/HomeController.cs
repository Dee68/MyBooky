using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using System.Diagnostics;

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
            IEnumerable<Product> products = _unitofWork.Product.GetAll(includeProperties: "Category");
            return View(products);
        }
        public IActionResult Details(int id)
        {
            Product product = _unitofWork.Product.Get(u => u.Id==id,includeProperties: "Category");
            return View(product);
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
