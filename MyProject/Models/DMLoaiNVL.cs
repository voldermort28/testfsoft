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
    
    public partial class DMLoaiNVL
    {
        public DMLoaiNVL()
        {
            this.DMNVLs = new HashSet<DMNVL>();
        }
    
        public int LoaiNVLID { get; set; }
        public string MaLoaiNVL { get; set; }
        public string TenLoaiNVL { get; set; }
        public string MoTa { get; set; }
    
        public virtual ICollection<DMNVL> DMNVLs { get; set; }
    }
}
