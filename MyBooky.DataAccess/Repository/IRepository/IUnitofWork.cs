namespace MyBooky.DataAccess.Repository.IRepository
{
  public interface IUnitofWork
  {
	ICategoryRepository Category { get; }
	IApplicationUserRepository ApplicationUser { get; }
	IShoppingCartRepository ShoppingCart { get; }
	IOrderDetailRepository OrderDetail { get; }
	IOrderHeadRepository OrderHead { get; }
	ICompanyRepository Company { get; }
	IProductImageRepository ProductImage { get; }
	IProductRepository Product { get; }
	void Save();
  }
}