using Microsoft.AspNetCore.Mvc;

namespace MyBookyWeb.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
