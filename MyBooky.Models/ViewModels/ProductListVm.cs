using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBooky.Models.ViewModels
{
    public class ProductListVm
    {
        public List<Product> Products { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string Query { get; set; } // Add a property to hold the search query

        public ProductListVm()
        {
            Query = string.Empty;
        }
    }
}
