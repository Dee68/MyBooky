using MyBooky.Models;

namespace MyBooky.DataAccess.Repository.IRepository
{
  public interface ICompanyRepository : IRepository<Company>
  {
	void Update(Company company);
  }
}