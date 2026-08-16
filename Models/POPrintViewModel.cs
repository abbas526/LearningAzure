using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class POPrintViewModel
    {
        public PurchaseOrder PO { get; set; }
        public List<POItemsViewModel> POItemsVM { get; set; }

        public POCompany POCompany { get; set; }
        public Vendor POVendor { get; set; }
    }
}