using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class Vendor
    {
        public string VendorName { get; set; }
        public string ContactPerson { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string GST { get; set; }
        public string IsActive { get; set; }
        public string Result { get; set; }
        public string VendorType { get; set; }
        public string VendorMSME { get; set; }
    }
}