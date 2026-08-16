using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class OutgoingChallanItem
    {
        public string ChallanNumber { get; set; }
        public string ItemDescription { get; set; }
        public string HSN { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string Amount { get; set; }
        public string ProjectName { get; set; }
    }
}