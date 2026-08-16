using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class PurchaseRequisition
    {
        public string PRNo { get; set; }
        public string PRDate { get; set; }
        public string ProjectRefDropdown { get; set; }
        public string ItemDropdown { get; set; }
        public string NewItem { get; set; }
        public string ItemSize { get; set; }
        public string Specs { get; set; }
        public string Quantity { get; set; }
        public string DateRequired { get; set; }
        public string Remark { get; set; }
        public string UserCode { get; set; }          
        public string Unit { get; set; }
        public string IsNew { get; set; }
        public string Result { get; set; }
        public string AssignedToPO { get; set; }

        public string BillNo { get; set; }
        public string Rate { get; set; }
        public string IsActive { get; set; }
        public string IsAllItemReceived { get; set; }
        public List<ItemReceived> ItemReceivedList { get; set; }
        public string AssociatedPONumbers { get; set; }
        public string Drawing { get; set; }

       // public string ItemReceived { get; set; }
        //public string ItemReceivedOK { get; set; }
        //public string ItemReceivedComment { get; set; }
        //public string ItemReceivedChallanNo { get; set; }
        //public string Vendor { get; set; }


    }

    public enum PRColumnNo
    {        
        PRDateDay = 1,
        PRDateMonth = 2,
        PRDateYear = 3,
        Item = 4,
        Size = 5,
        Specs= 6,
        Qty= 7,
        Unit = 8,
        Remark = 9,
        DateReqdDay = 10,
        DateReqdMonth = 11,
        DateReqdYear = 12,        
        UserCode = 13,
        Drawing = 14
    }



}
