using MyBooky.DataAccess.Repository.IRepository;
using MyBookyWeb.DataAccess.Data;

namespace MyBooky.DataAccess.Repository
{
    public class UnitofWork : IUnitofWork
    {
        private ApplicationDbContext _db;
        public ICategoryRepository Category { get; }
        public IApplicationUserRepository ApplicationUser { get; set; }
        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IOrderHeadRepository OrderHead { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IProductRepository Product { get; }
        public IProductImageRepository ProductImage { get; private set; }
        public UnitofWork(ApplicationDbContext db)
        {
            _db = db;
            ProductImage = new ProductImageRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            OrderHead = new OrderHeadRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);

        }


        public void Save()
        {

            _db.SaveChanges();
        }
    }
}