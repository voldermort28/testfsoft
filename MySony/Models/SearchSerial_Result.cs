//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MySony.Models
{
    using System;
    
    public partial class SearchSerial_Result
    {
        public int serial_id { get; set; }
        public string modelname { get; set; }
        public string serialnumber { get; set; }
        public string productcode { get; set; }
        public string warrantycardnumber { get; set; }
        public string partcode { get; set; }
        public Nullable<System.DateTime> manufacturingdate { get; set; }
        public string batterynumber { get; set; }
        public string adapternumber { get; set; }
        public string alphalenumber { get; set; }
        public Nullable<decimal> period { get; set; }
        public Nullable<System.DateTime> expireddate { get; set; }
        public string RM { get; set; }
        public string RM1 { get; set; }
        public string RM2 { get; set; }
        public Nullable<System.DateTime> dateimport { get; set; }
        public Nullable<int> status_id { get; set; }
    }
}
