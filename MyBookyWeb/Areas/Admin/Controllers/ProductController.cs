using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.DataAccess.Repository;
using MyBooky.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyBooky.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace MyBookyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
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
                productVm.Product = _unitofWork.Product.Get(u => u.Id == id, includeProperties:"ProductImages");
                return View(productVm);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVm productVm, List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
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
                //get www root path
                string wwwRoothPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        //append product id to image file
                        string productPath = @"images\product" + productVm.Product.Id;
                        string finalPath = Path.Combine(wwwRoothPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);
                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = productVm.Product.Id,
                        };

                        if (productVm.Product.ProductImages == null)
                            productVm.Product.ProductImages = new List<ProductImage>();

                        productVm.Product.ProductImages.Add(productImage);

                    }

                    _unitofWork.Product.Update(productVm.Product);
                    _unitofWork.Save();
                    
                }

                TempData["success"] = "Product created/updated successfully";
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

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitofWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(_webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitofWork.ProductImage.Remove(imageToBeDeleted);
                _unitofWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
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
            var productToBeDeleted = _unitofWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            _unitofWork.Product.Remove(productToBeDeleted);
            _unitofWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
