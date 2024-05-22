using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBookyWeb.DataAccess.Data;


namespace MyBooky.DataAccess.Repository
{
  public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
  {
	private readonly ApplicationDbContext _db;
	public ProductImageRepository(ApplicationDbContext db) : base(db)
	{
	  _db = db;
	}

	public void Update(ProductImage productImage)
	{
	  _db.ProductImages.Update(productImage);
	}
  }
}