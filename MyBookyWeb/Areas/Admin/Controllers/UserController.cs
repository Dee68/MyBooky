using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
  public class UserController(UserManager<IdentityUser> userManager, IUnitofWork unitofWork, RoleManager<IdentityRole> roleManager) : Controller
  {
	private readonly UserManager<IdentityUser> _userManager = userManager;
	private readonly RoleManager<IdentityRole> _roleManager = roleManager;
	private readonly IUnitofWork _unitofWork = unitofWork;

	public IActionResult Index()
	{
	  return View();
	}
	public IActionResult RoleManagement(string userId)
	{

	  RoleManagementVM RoleVM = new()
	  {
		ApplicationUser = _unitofWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company"),
		RoleList = _roleManager.Roles.Select(i => new SelectListItem
		{
		  Text = i.Name,
		  Value = i.Name
		}),
		CompanyList = _unitofWork.Company.GetAll().Select(i => new SelectListItem
		{
		  Text = i.Name,
		  Value = i.Id.ToString()
		}),
	  };

	  RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitofWork.ApplicationUser.Get(u => u.Id == userId))
			  .GetAwaiter().GetResult().FirstOrDefault();
	  return View(RoleVM);
	}

	[HttpPost]
	public IActionResult RoleManagement(RoleManagementVM roleManagmentVM)
	{
	  var user = _unitofWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id) ?? throw new InvalidOperationException("User not found.");
	  var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
	  var oldRole = roles.FirstOrDefault();

	  ApplicationUser applicationUser = _unitofWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);


	  if (roleManagmentVM.ApplicationUser.Role != oldRole)
	  {
		//a role was updated
		if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company)
		{
		  applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
		}
		if (oldRole == SD.Role_Company)
		{
		  applicationUser.CompanyId = null;
		}
		_unitofWork.ApplicationUser.Update(applicationUser);
		_unitofWork.Save();

		_userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
		_userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();

	  }
	  else
	  {
		if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId)
		{
		  applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
		  _unitofWork.ApplicationUser.Update(applicationUser);
		  _unitofWork.Save();
		}
	  }

	  return RedirectToAction("Index");
	}

	#region API CALLS

	[HttpGet]
	public IActionResult GetAll()
	{
	  List<ApplicationUser> objUserList = _unitofWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();

	  foreach (var user in objUserList)
	  {

		user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

		user.Company ??= new Company()
		{
		  Name = ""
		};
	  }

	  return Json(new { data = objUserList });
	}


	[HttpPost]
	public IActionResult LockUnlock([FromBody] string id)
	{

	  var objFromDb = _unitofWork.ApplicationUser.Get(u => u.Id == id);
	  if (objFromDb == null)
	  {
		return Json(new { success = false, message = "Error while Locking/Unlocking" });
	  }

	  if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
	  {
		//user is currently locked and we need to unlock them
		objFromDb.LockoutEnd = DateTime.Now;
	  }
	  else
	  {
		objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
	  }
	  _unitofWork.ApplicationUser.Update(objFromDb);
	  _unitofWork.Save();
	  return Json(new { success = true, message = "Operation Successful" });
	}

	[HttpPost]
	public IActionResult Delete(string userId)
	{
	  var user = _unitofWork.ApplicationUser.Get(u => u.Id == userId);
	  if (user == null)
	  {
		return Json(new { success = false, message = "User not found" });
	  }
	  _unitofWork.ApplicationUser.Remove(user);
	  _unitofWork.Save();

	  return Json(new { success = true, message = "User deleted successfully" });
	}


	#endregion
  }
}