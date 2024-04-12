using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.DataAccess.Repository;
using MyBooky.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyBooky.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace MyBookyWeb.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitofWork unitofWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitofWork = unitofWork;
            this._webHostEnvironment = webHostEnvironment;

        }
        public IActionResult Index()
        {
            List<Product> products = _unitofWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(products);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVm productVm = new()
            {
                CategoryList = _unitofWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create
                return View(productVm);
            }
            else
            {
                //update
                productVm.Product = _unitofWork.Product.Get(u => u.Id == id);
                return View(productVm);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVm productVm, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                //get www root path
                string wwwRoothPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    //get a random number for image name
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    //set the path to save image
                    string productPath = Path.Combine(wwwRoothPath, @"images\product");
                    if (!string.IsNullOrEmpty(productVm.Product.ImageUrl))
                    {
                        //delete old image
                        var oldImagePath = Path.Combine(wwwRoothPath, productVm.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);

                        }

                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    // get and save image url to product
                    productVm.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (productVm.Product.Id == 0)
                {
                    _unitofWork.Product.Add(productVm.Product);
                    TempData["success"] = "Product successfully created.";
                }
                else
                {
                    _unitofWork.Product.Update(productVm.Product);
                    TempData["success"] = "Product successfully updated.";
                }

                _unitofWork.Save();

                return RedirectToAction("Index");
            }
            else
            {
                productVm.CategoryList = _unitofWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVm);
            }

        }

       

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> productList = _unitofWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToDelete = _unitofWork.Product.Get(u => u.Id == id);
            if (productToDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting product" });
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToDelete.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);

            }
            _unitofWork.Product.Remove(productToDelete);
            _unitofWork.Save();
            return Json(new { success = true, message = "Product successfully deleted" });

        }
        #endregion
    }
}
