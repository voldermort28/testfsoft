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
    
    public partial class DMLoaiHang
    {
        public DMLoaiHang()
        {
            this.DMHangHoas = new HashSet<DMHangHoa>();
        }
    
        public int LoaiHangID { get; set; }
        public string MaLoaiHang { get; set; }
        public string TenLoaiHang { get; set; }
        public string MoTa { get; set; }
    
        public virtual ICollection<DMHangHoa> DMHangHoas { get; set; }
    }
}
