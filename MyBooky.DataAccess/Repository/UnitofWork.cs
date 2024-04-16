using MyBooky.DataAccess.Repository.IRepository;
using MyBookyWeb.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBooky.DataAccess.Repository
{
    public class UnitofWork : IUnitofWork
    {
        private readonly ApplicationDbContext _db;
        public ICategoryRepository Category { get; }
        public IApplicationUserRepository ApplicationUser { get; set; }
        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IProductRepository Product { get; }
        public IProductImageRepository ProductImage { get; private set; }
        public UnitofWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            ProductImage = new ProductImageRepository(_db);
            Company = new CompanyRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
        }
        

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
