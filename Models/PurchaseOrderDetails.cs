using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class PurchaseOrderDetails
    {
        public string ProjectName { get; set; }
        public string PONumber { get; set; }
        public string PRNumber { get; set; }
        public string Rate { get; set; }
        public string Discount { get; set; }
        public string POQuantity { get; set; }
        public string PRQty { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string PODate { get; set; }
        public string Vendor { get; set; }
        public string IsActive { get; set; }
    }
}