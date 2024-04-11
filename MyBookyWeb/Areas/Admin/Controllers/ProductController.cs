using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.DataAccess.Repository;
using MyBooky.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyBookyWeb.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        public ProductController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult Index()
        {
            List<Product> products = _unitofWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(products);
        }
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> categoryList = _unitofWork.Category.GetAll().Select(u =>
            new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            ViewBag.CategoryList = categoryList;
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product product)
        {

            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? productFromDb = _unitofWork.Product.Get(u => u.Id == id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _unitofWork.Product.Update(product);
                _unitofWork.Save();
                TempData["success"] = "Product successfully updated";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
