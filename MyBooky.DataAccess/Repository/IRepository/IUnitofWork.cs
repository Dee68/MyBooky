using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyBooky.DataAccess.Repository.IRepository;

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
