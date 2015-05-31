using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySony.ViewModels
{
    public class ListCustomerProductVM
    {
        List<CustomerProductVM> listCusVM { get; set; }
    }

    public class CustomerProduct
    {
        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }
        public string ProductName { get; set; }
        public string Serial { get; set; }
        public DateTime? TimeWarranty { get; set; }
        public DateTime? TimePucharsed { get; set; }
        public string ShopName { get; set; }
        public int CategoryProductId { get; set; }
    }

}