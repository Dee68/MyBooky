using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBooky.Utilities;
using MyBookyWeb.DataAccess.Data;

namespace MyBookyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        public CategoryController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult Index()
        {
            List<Category> categories = _unitofWork.Category.GetAll().ToList();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            //Check if category name already in database
            if (_unitofWork.Category.GetAll().Any(Category => Category.Name == obj.Name))
            {
                ModelState.AddModelError("name", "Category already exists.");
            }
            if (ModelState.IsValid)
            {
                _unitofWork.Category.Add(obj);
                _unitofWork.Save();
                TempData["success"] = "Category successfully created";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitofWork.Category.GetById(id.Value);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitofWork.Category.Update(obj);
                _unitofWork.Save();
                TempData["success"] = "Category successfully updated";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitofWork.Category.GetById(id.Value);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category categoryFromDb = _unitofWork.Category.GetById(id);
            if (categoryFromDb == null)
            {
                return NotFound(id);
            }
            _unitofWork.Category.Remove(categoryFromDb);
            _unitofWork.Save();
            TempData["success"] = "Category successfully deleted";
            return RedirectToAction("Index");
        }
    }
}
