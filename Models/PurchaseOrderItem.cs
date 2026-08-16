using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class PurchaseOrderItem
    {
        public string ProjectName { get; set; }
        public string PONumber { get; set; }
        public string PRNumber { get; set; }
        public string Rate { get; set; }
        public string Discount { get; set; }
        public string POQuantity { get; set; }
        public string Qty { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string Drawing { get; set; }
        public string ItemSize { get; set; }
        public string PODate { get; set; }
    }
    public enum POItemColumnNo
    {
        PONumber = 0,
        PRNumber = 1,
        Rate = 2,
        Discount = 3,
        Quantity = 4
    }
}