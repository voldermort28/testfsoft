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
    
    public partial class Reservation
    {
        public int ReservationId { get; set; }
        public string ReservationName { get; set; }
        public Nullable<int> ReservationPaid { get; set; }
        public int ReservationStatus { get; set; }
        public System.DateTime ReservationStart { get; set; }
        public System.DateTime ReservationEnd { get; set; }
        public bool Status { get; set; }
        public int BanID { get; set; }
    }
}
