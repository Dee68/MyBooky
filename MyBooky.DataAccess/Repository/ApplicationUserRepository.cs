using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBookyWeb.DataAccess.Data;

namespace MyBooky.DataAccess.Repository
{
  public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
  {
	private readonly ApplicationDbContext _db;
	public ApplicationUserRepository(ApplicationDbContext db) : base(db)
	{
	  _db = db;
	}
	public void Update(ApplicationUser applicationUser)
	{
	  _db.ApplicationUsers.Update(applicationUser);
	}
  }
}