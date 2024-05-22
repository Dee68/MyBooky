namespace MyBooky.Models.ViewModels
{
  public class OrderVm
  {
    public OrderHead OrderHead { get; set; }
    public IEnumerable<OrderDetail> OrderDetail { get; set; }

  }
}