using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBooky.Utilities;
using MyBooky.Models;
using MyBookyWeb.DataAccess.Data;

namespace MyBooky.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
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
                // Log the exception or handle it in a meaningful way
                Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
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

            // Check if admin user exists
            if (_userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult() == null)
            {
                // Generate a random temporary password
                var temporaryPassword = GenerateTemporaryPassword();

                // Create admin user
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Dimie Egberipou",
                    PhoneNumber = "1112223333",
                    StreetAddress = "test 123 Ave",
                    State = "Bayelsa",
                    PostalCode = "23422",
                    City = "Yenegoa"
                };

                var result = _userManager.CreateAsync(adminUser, temporaryPassword).GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    // Assign admin role to admin user
                    _userManager.AddToRoleAsync(adminUser, SD.Role_Admin).GetAwaiter().GetResult();
                }
                else
                {
                    throw new ApplicationException($"Unable to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

        }
        private string GenerateTemporaryPassword()
        {
            // Generate a random temporary password (you can use any logic here)
            //var temporaryPassword = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            // For example, you can use a library like System.Security.Cryptography to generate a random password
            return "Temp@1432"; // Change this to use your actual password generation logic
        }
    } 
}

