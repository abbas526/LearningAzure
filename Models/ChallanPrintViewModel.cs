using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class ChallanPrintViewModel
    {
        public OutgoingChallan Challan { get; set; }       

        public POCompany POCompany { get; set; }
        public Vendor POVendor { get; set; }
    }
}