using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class PurchaseOrder
    {
        public string UserCode { get; set; }        
        public string PODate { get; set; }
        public string PONumber { get; set; }
        public string Company { get; set; }
        public string ProjectRef { get; set; }

        public string Vendor { get; set; }      
        
        public string QuoteRef { get; set; }
        public string HSNNo { get; set; }

        public string PORemarks { get; set; }
        public string DeliveryRequiredBy { get; set; }
        public string DeliveryInstructions { get; set; }
        public string DeliveryRequiredAt { get; set; }        
        public string TransportationCharges { get; set; }
        public string PaymentTerms { get; set; }
        public string AllProjects { get; set; } // all project names seperated by a comma

        public string POAmount { get; set; }

        public List<PurchaseOrderItem> PurchaseOrderItems { get; set; }

        public string IsActive { get; set; }

        public string BillAmount { get; set; }
        public string BillNoAndDate { get; set; }
        public string PaymentDate { get; set; }

        public string DisplayDiscount { get; set; }
        public string DisplayTotal { get; set; }

        public string IsApproved { get; set; }
        public string IsApprovalRequested { get; set; }

    }



    public enum POColumnNo
    {        
        PONumber=0,
        PODate = 1,
        VendorObj =2,
        QuoteRef=3,
        HSNNo=4,       
        PORemarks=5,
        DeliveryRequiredBy =6,
        DeliveryInstructions=7,
        DeliveryRequiredAt=8,
        TransportationCharges=9,
        PaymentTerms=10,
        UserCode=11,
        Company=12,
        AllProjects=13,
        POAmount=14
    }

    //public string Rate { get; set; }
    //public string ExpectedDate { get; set; }
    //public string Variations { get; set; }
    //public string ExcessQuantity { get; set; }
    //public string ExcessQuantityRemarks { get; set; }
    //public string ChallanNo { get; set; }
    //public string ReceivedDate { get; set; }

    //public string ChallanDate { get; set; }
    //public string InvoiceNo { get; set; }
    //public string InvoiceDate { get; set; }
    //public string InvoiceAmount { get; set; }
    //public string ChequeNo { get; set; }

    //public string ChequeDate { get; set; }




}