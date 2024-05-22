using System.ComponentModel.DataAnnotations;

namespace MyBooky.Models
{
  public class Company
  {
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? PhoneNumber { get; set; }

    public Company()
    {
      // Initialize the Name property with a non-null value
      Name = string.Empty;
    }
  }
}