using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Models.ViewModels;
using MyBooky.Utilities;

namespace MyBookyWeb.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize(Roles = SD.Role_Admin)]
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
	  List<Product> products = _unitofWork.Product.GetAll(includeProperties: "Category").ToList();
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
		Product = new Product(),
		IdInDatabase = true
	  };
	  // Check if productVm is null
	  if (productVm == null)
	  {
		// Handle the case where productVm is null
		TempData["error"] = "Error creating/updating product: ProductVm is null.";
		return RedirectToAction("Index");
	  }
	  if (id == null || id == 0)
	  {
		//create
		return View(productVm);
	  }
	  else
	  {
		var existingProduct = _unitofWork.Product.Get(u => u.Id == id, includeProperties: "ProductImages");
		if (existingProduct == null)
		{
		  // Product with the given ID does not exist in the database
		  TempData["error"] = "Product not found.";
		  productVm.IdInDatabase = false;
		  return View(productVm);
		}

		// Populate productVm with existing product data
		productVm.Product = existingProduct;

		productVm.IdInDatabase = true;
		return View(productVm);
	  }
	}
	[HttpPost]
	public IActionResult Upsert(ProductVm productVm, List<IFormFile>? files)
	{
	  if (productVm == null)
	  {
		// Handle the case where productVm is null
		TempData["error"] = "Bad request";
		return RedirectToAction("Index");
	  }
	  if (ModelState.IsValid)
	  {
		if (productVm.Product == null)
		{
		  // Handle the case where productVm.Product is null
		  TempData["error"] = "Bad request";
		  return RedirectToAction("Index");
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