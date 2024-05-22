using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBookyWeb.DataAccess.Data;

namespace MyBooky.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;

        }


        public void Update(OrderDetail orderDetail)
        {
            throw new NotImplementedException();
        }
    }
}