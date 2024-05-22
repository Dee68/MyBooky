using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyBooky.Models.ViewModels
{
  public class RoleManagementVM
  {
    public ApplicationUser ApplicationUser { get; set; }
    public IEnumerable<SelectListItem> RoleList { get; set; }
    public IEnumerable<SelectListItem> CompanyList { get; set; }

    public RoleManagementVM()
    {
      IEnumerable<SelectListItem> value = [];
      RoleList = value;
      CompanyList = [];
      ApplicationUser = new ApplicationUser();
    }
  }
}