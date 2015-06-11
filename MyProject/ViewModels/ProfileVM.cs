using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyProject.Models;
namespace MyProject.ViewModels
{
    public class ProfileVM
    {
        public int NhomMenuID { get; set; }
        public string MaMenu { get; set; }
        public string MaNhomNguoiDung { get; set; }
        public string Sex { get; set; }
        public DateTime? Birthday { get; set; }
        
    }
}
  