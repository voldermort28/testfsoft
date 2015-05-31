using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySony.Models;
namespace MySony.ViewModels
{
    public class ProfileVM
    {
        public int ID { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public DateTime? RegDate { get; set; }
        public string Status { get; set; }
        public int? status_id { get; set; }
        public string IsActive { get; set; }
        public int? status_active { get; set; }
        // for customer products
        public string Product { get; set; }
        public string Serial { get; set; }
        public string Shop { get; set; }
        public DateTime? BuyDate { get; set; }
        public DateTime? WarrantyEnd { get; set; }


        public String RegDateString { get; set; }
        public String ByDateString { get; set; }
        public String WarrantyEndString { get; set; }
        public String BirthdayString { get; set; }
    }
}