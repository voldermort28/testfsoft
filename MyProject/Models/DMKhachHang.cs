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
    
    public partial class DMKhachHang
    {
        public DMKhachHang()
        {
            this.ChiTietPhieuNhaps = new HashSet<ChiTietPhieuNhap>();
            this.ChiTietPhieuXuats = new HashSet<ChiTietPhieuXuat>();
            this.HoaDons = new HashSet<HoaDon>();
            this.PhieuChis = new HashSet<PhieuChi>();
            this.PhieuThus = new HashSet<PhieuThu>();
        }
    
        public long KhachHangID { get; set; }
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public Nullable<bool> GioTinh { get; set; }
        public Nullable<System.DateTime> NgaySinh { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public Nullable<int> LoaiKhachHangID { get; set; }
        public Nullable<int> DiemTich { get; set; }
        public string MaSoThue { get; set; }
        public string DiaChi { get; set; }
        public string CardID { get; set; }
        public Nullable<System.DateTime> NgayCap { get; set; }
        public Nullable<double> DinhMucCongNo { get; set; }
        public string NguoiTao { get; set; }
        public Nullable<System.DateTime> NgayTao { get; set; }
        public string NguoiSua { get; set; }
        public Nullable<System.DateTime> NgaySua { get; set; }
    
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
        public virtual DMLoaiKhachHang DMLoaiKhachHang { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ICollection<PhieuChi> PhieuChis { get; set; }
        public virtual ICollection<PhieuThu> PhieuThus { get; set; }
    }
}
