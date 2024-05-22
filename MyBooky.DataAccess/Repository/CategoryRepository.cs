using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBookyWeb.DataAccess.Data;

namespace MyBooky.DataAccess.Repository
{
  public class CategoryRepository : Repository<Category>, ICategoryRepository
  {
	private readonly ApplicationDbContext _db;
	public CategoryRepository(ApplicationDbContext db) : base(db)
	{
	  _db = db;
	}



	public void Update(Category category)
	{
	  _db.Update(category);
	}
  }
}