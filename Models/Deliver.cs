//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CafateriaSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Deliver
    {
        public int deliver_id { get; set; }
        public int order_id { get; set; }
        public int customer_id { get; set; }
        public int driver_id { get; set; }
        public System.TimeSpan dispatch_time { get; set; }
        public string deliver_status { get; set; }
    
        public virtual Order Order { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
