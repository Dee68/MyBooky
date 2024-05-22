using System.Linq.Expressions;

namespace MyBooky.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //T - Category
        T GetById(int id);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
        void Add(T item);
        void Remove(T item);
        void RemoveById(int id);
        void RemoveRange(IEnumerable<T> items);
    }
}