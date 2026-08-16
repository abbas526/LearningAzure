using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class OutgoingChallan
    {
        public string ChallanNumber { get; set; }
        public string ChallanDate { get; set; }
        public string VehicleNumber { get; set; }
        public string EWayBillNo { get; set; }
        public string Vendor { get; set; }

        public string Company { get; set; }

        public List<OutgoingChallanItem> OutgoingChallanItems { get; set; }

        public string FinalAmount { get; set; }

        public string Comment { get; set; }

        public string ReceivedDate { get; set; }
        public string ReceivedComment { get; set; }
        public string ValueOfMaterial { get; set; }
        public string VendorGST { get; set; }

        public string Result { get; set; }

    }

    public class OutgoingChallanWithItem
    {
        public string ChallanNumber { get; set; }
        public string ChallanDate { get; set; }
        public string VehicleNumber { get; set; }
        public string EWayBillNo { get; set; }
        public string Vendor { get; set; }
        public string Company { get; set; }
        public string ItemDescription { get; set; }
        public string HSN { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string Amount { get; set; }
        public string ProjectName { get; set; }
        public string ValueOfMaterial { get; set; }
        public string Comment { get; set; }
        public string ReceivedDate { get; set; }
        public string ReceivedComment { get; set; }


    }
}
