using MyBooky.Models;

namespace MyBooky.DataAccess.Repository.IRepository
{
  public interface IProductRepository : IRepository<Product>
  {
	void Update(Product product);
  }
}