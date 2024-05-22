namespace MyBooky.Models.ViewModels
{
  public class ShoppingCartVm
  {
	public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
	public OrderHead OrderHead { get; set; }
  }
}