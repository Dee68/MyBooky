using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBooky.Models;
using MyBooky.Utilities;
using MyBookyWeb.DataAccess.Data;

namespace MyBooky.DataAccess.DbInitializer
{
  public class DbInitializer(
		UserManager<IdentityUser> userManager,
		RoleManager<IdentityRole> roleManager,
		ApplicationDbContext db,
		ILogger<DbInitializer> logger
			) : IDbInitializer
  {
	private readonly UserManager<IdentityUser> _userManager = userManager;
	private readonly RoleManager<IdentityRole> _roleManager = roleManager;
	private readonly ApplicationDbContext _db = db;
	private readonly ILogger<DbInitializer> _logger = logger;
	public void Initialize()
	{
	  //migrations if they are not applied
	  try
	  {
		// Apply pending migrations
		if (_db.Database.GetPendingMigrations().Any())
		{
		  _db.Database.Migrate();
		}
		//Create roles
		CreateRoles();
		// Create admin user if it does not exist
		CreateAdminUser();
	  }
	  catch (Exception ex)
	  {
		// Log the exception
		_logger.LogError(ex, "An error occurred during database initialization");
	  }



	}

	private void CreateRoles()
	{
	  foreach (var roleName in new[] { SD.Role_Customer, SD.Role_Employee, SD.Role_Admin, SD.Role_Company })
	  {
		if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
		{
		  _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
		}
	  }
	}
	private void CreateAdminUser()
	{
	  var adminEmail = "admin@gmail.com";

	  // Check if admin user does not exists
	  if (_userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult() == null)
	  {
		// Generate a random temporary password
		var temporaryPassword = GenerateTemporaryPassword;
		// Create admin user
		var adminUser = new ApplicationUser
		{
		  UserName = adminEmail,
		  Email = adminEmail,
		  Name = "Admin Admin",
		  PhoneNumber = "24242424242",
		  StreetAddress = "admin 123 Ave",
		  State = "Bayelsa",
		  PostalCode = "24424",
		  City = "Yenegoa",
		  EmailConfirmed = true
		};

		var result = _userManager.CreateAsync(adminUser, temporaryPassword).GetAwaiter().GetResult();

		if (result.Succeeded)
		{
		  // Assign admin role to admin user
		  _userManager.AddToRoleAsync(adminUser, SD.Role_Admin).GetAwaiter().GetResult();
		}
		else
		{
		  string? description = result.Errors.Select(x => x.Description).FirstOrDefault();
		  _logger.LogError("Unable to create admin user: {Description} ", description);
		}
	  }

	}
	private static string GenerateTemporaryPassword => "Temp@1432"; // Change this later to use your actual password
  }
}