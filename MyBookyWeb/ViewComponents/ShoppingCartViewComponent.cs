using Microsoft.AspNetCore.Mvc;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Utilities;
using System.Security.Claims;

namespace MyBookyWeb.ViewComponents
{
  public class ShoppingCartViewComponent(IUnitofWork unitofWork) : ViewComponent
  {
	private readonly IUnitofWork _unitofWork = unitofWork;

	public async Task<IViewComponentResult> InvokeAsync()
	{
	  ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
	  var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

	  if (claim != null)
	  {

		if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
		{
		  HttpContext.Session.SetInt32(SD.SessionCart,
		  _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count());
		}

		return View(HttpContext.Session.GetInt32(SD.SessionCart));
	  }
	  else
	  {
		HttpContext.Session.Clear();
		return View(0);
	  }
	}
  }
}