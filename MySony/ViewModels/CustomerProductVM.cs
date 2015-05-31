using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProject.ViewModels
{
    public class CustomerProductVM
    {
        public int ID { get; set; }
        public int? CustomerID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int CategoryID { get; set; }
        public string Product { get; set; }
        public string Serial { get; set; }
        public string Shop { get; set; }
        public int ShopID { get; set; }
        public int CityID { get; set; }
        public DateTime? BuyDate { get; set; }
        public DateTime? WarrantyEnd { get; set; }
    }
}