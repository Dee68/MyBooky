using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBooky.Models.ViewModels
{
    public class OrderVm
    {
        public OrderHead OrderHead { get; set; }
        public IEnumerable<OrderDetail> OrderDetail { get; set; }

        //public OrderVm()
        //{
        //    OrderDetail = new List<OrderDetail>();
        //    OrderHead = new OrderHead();
        //}

    }
}
