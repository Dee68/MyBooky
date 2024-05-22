using MyBooky.Models;

namespace MyBooky.DataAccess.Repository.IRepository
{
  public interface IOrderDetailRepository : IRepository<OrderDetail>
  {
	void Update(OrderDetail orderDetail);
  }
}