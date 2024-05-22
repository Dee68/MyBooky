using MyBooky.Models;

namespace MyBooky.DataAccess.Repository.IRepository
{
    public interface IOrderHeadRepository : IRepository<OrderHead>
    {
        void Update(OrderHead orderHead);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);

    }
}