using Microsoft.AspNetCore.Mvc;
using MyBooky.Models;
using MyBookyWeb.DataAccess.Data;

namespace MyBookyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<Category> categories = _db.Categories.ToList();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if(_db.Categories.Any<Category>(Category => Category.Name == obj.Name))
            {
                ModelState.AddModelError("name", "Category already exists.");
            }
            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();
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
            Category? categoryFromDb = _db.Categories.Where<Category>(Category => Category.Id == id).FirstOrDefault();
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            //if (_db.Categories.Any<Category>(Category => Category.Name == obj.Name))
            //{
            //    ModelState.AddModelError("name", "Category already exists.");
            //}
            if (ModelState.IsValid)
            {
                _db.Update(obj);
                _db.SaveChanges(true);
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
            Category? categoryFromDb = _db.Categories.Where<Category>(Category=>Category.Id == id).FirstOrDefault();
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {

            Category? categoryFromDb = _db.Categories.Find(id);
            if (categoryFromDb == null)
            {
                return NotFound(id);
            }
            _db.Categories.Remove(categoryFromDb);
            _db.SaveChanges();
            TempData["success"] = "Category successfully deleted";
            return RedirectToAction("Index");
        }
    }
}
