using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBookyWeb.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBooky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db; 
        }
        public void Update(Product product)
        {
            _db.Update(product);
        }
    }
}
