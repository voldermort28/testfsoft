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
    
    public partial class customertype
    {
        public customertype()
        {
            this.customers = new HashSet<customer>();
        }
    
        public int customertype_id { get; set; }
        public string name { get; set; }
        public Nullable<int> status_id { get; set; }
    
        public virtual ICollection<customer> customers { get; set; }
        public virtual status status { get; set; }
    }
}
