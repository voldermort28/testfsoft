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
    
    public partial class ChiTietPhieuNhap
    {
        public long PhieuNhapID { get; set; }
        public int NVLID { get; set; }
        public Nullable<long> NhaCungCap { get; set; }
        public Nullable<double> SoLuong { get; set; }
        public Nullable<int> DonViTinh { get; set; }
        public string MoTa { get; set; }
    
        public virtual DMKhachHang DMKhachHang { get; set; }
        public virtual DMNVL DMNVL { get; set; }
        public virtual PhieuNhap PhieuNhap { get; set; }
    }
}
