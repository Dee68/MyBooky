using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyBookyWeb.Areas.Identity.Pages.Account
{
  public class LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger) : PageModel
  {
	private readonly SignInManager<IdentityUser> _signInManager = signInManager;
	private readonly ILogger<LogoutModel> _logger = logger;

	public async Task<IActionResult> OnPost(string? returnUrl = null)
	{
	  await _signInManager.SignOutAsync();
	  HttpContext.Session.Clear();
	  _logger.LogInformation("User logged out.");
	  if (returnUrl != null)
	  {
		return LocalRedirect(returnUrl);
	  }
	  else
	  {
		// This needs to be a redirect so that the browser performs a new
		// request and the identity for the user gets updated.
		return RedirectToPage();
	  }
	}
  }
}