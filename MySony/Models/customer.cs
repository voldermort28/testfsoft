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
    
    public partial class customer
    {
        public int customer_id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public Nullable<bool> sex { get; set; }
        public Nullable<System.DateTime> birthday { get; set; }
        public string homephone { get; set; }
        public string mobilephone { get; set; }
        public string address { get; set; }
        public Nullable<int> district_id { get; set; }
        public Nullable<int> city_id { get; set; }
        public string identitycard { get; set; }
        public Nullable<int> job_id { get; set; }
        public Nullable<int> education_id { get; set; }
        public Nullable<int> status_id { get; set; }
        public Nullable<int> customertype_id { get; set; }
        public string customercodereg { get; set; }
        public Nullable<System.DateTime> datereg { get; set; }
        public Nullable<int> status_active { get; set; }
        public Nullable<System.DateTime> modifieddate { get; set; }
        public string RM { get; set; }
        public string RM1 { get; set; }
        public string RM2 { get; set; }
        public Nullable<bool> IsPending { get; set; }
    
        public virtual city city { get; set; }
        public virtual district district { get; set; }
        public virtual education education { get; set; }
        public virtual job job { get; set; }
        public virtual status status { get; set; }
        public virtual customertype customertype { get; set; }
    }
}
