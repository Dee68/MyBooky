using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyBooky.Models.ViewModels
{
  public class ProductVm
  {
	public Product Product { get; set; }
	[ValidateNever]
	public IEnumerable<SelectListItem> CategoryList { get; set; }
	public bool IdInDatabase { get; set; }

  }
}