//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyProject.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ANhomNguoiDung
    {
        public ANhomNguoiDung()
        {
            this.ANguoiDungs = new HashSet<ANguoiDung>();
            this.ANhomMenus = new HashSet<ANhomMenu>();
        }
    
        public int NhomNguoiDungID { get; set; }
        public string MaNhomNguoiDung { get; set; }
        public string TenNhomNguoiDung { get; set; }
        public bool TrangThai { get; set; }
        public string MoTa { get; set; }
        public string NguoiDung { get; set; }
        public string ChucNang { get; set; }
        public System.DateTime NgayTao { get; set; }
        public string NguoiTao { get; set; }
        public Nullable<System.DateTime> NgaySua { get; set; }
        public string NguoiSua { get; set; }
        public Nullable<bool> Active { get; set; }
    
        public virtual ICollection<ANguoiDung> ANguoiDungs { get; set; }
        public virtual ICollection<ANhomMenu> ANhomMenus { get; set; }
    }
}
