using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;

namespace MyBookyWebUnitTest
{
    public class MockUnitofWorks : IUnitofWork
    {
        private List<Product> _products;
        private List<Category> _categoryList;

        public MockUnitofWorks(List<Product> products, List<Category> categoryList)
        {
            this._products = products;
            this._categoryList = categoryList;
        }

        public MockUnitofWorks(
        ICategoryRepository categoryRepository,
        IApplicationUserRepository applicationUserRepository,
        IShoppingCartRepository shoppingCartRepository,
        IOrderDetailRepository orderDetailRepository,
        IOrderHeadRepository orderHeadRepository,
        ICompanyRepository companyRepository,
        IProductImageRepository productImageRepository,
        IProductRepository productRepository)
        {
            Category = categoryRepository;
            ApplicationUser = applicationUserRepository;
            ShoppingCart = shoppingCartRepository;
            OrderDetail = orderDetailRepository;
            OrderHead = orderHeadRepository;
            Company = companyRepository;
            ProductImage = productImageRepository;
            Product = productRepository;
        }

        public ICategoryRepository Category { get; }
        public IApplicationUserRepository ApplicationUser { get; }
        public IShoppingCartRepository ShoppingCart { get; }
        public IOrderDetailRepository OrderDetail { get; }
        public IOrderHeadRepository OrderHead { get; }
        public ICompanyRepository Company { get; }
        public IProductImageRepository ProductImage { get; }
        public IProductRepository Product { get; }

        public void Save()
        {
            // No implementation needed for testing purposes
        }
    }
}