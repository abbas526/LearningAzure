using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{

    public class BillDetail
    {
        public string BillNo { get; set; }
        public bool FullPaymentDone { get; set; }
        public string Amount { get; set; }
    }

    public class PaymentViewModel
    {
        public string Vendor { get; set; }
        public string PaymentAmount { get; set; }
        public string PaymentDate { get; set; }
        public string OnlineRefNo { get; set; }
        public string ChequeNumber { get; set; }
        public List<BillDetail> BillDetails { get; set; }
    }

    public class BillsAndPaymentModel
    {
        public string Result { get; set; }
        public string Vendor { get; set; }
        public string ChallanNo { get; set; }
        public string BillNo { get; set; }
        public string BillAmount { get; set; }
        public string BillDate { get; set; }
        public string PaymentAmount { get; set; }
        public string BalanceAmount { get; set; }
        public string PaymentDate { get; set; }
        public string ChequeNo { get; set; }
        public string OnlinePaymentRefNo { get; set; }
    }

    public class VendorPayments
    {
        public string Result { get; set; }
        public string Vendor { get; set; }

        public string PaymentId { get; set; }
        public string PaymentAmount { get; set; }
        public string PaymentDate { get; set; }
        public string ChequeNo { get; set; }
        public string OnlinePaymentRefNo { get; set; }
        public string BillNo { get; set; }
        public List<BillModel> bills { get; set; }
        public string BillList { get; set; }

    }

    public class BillModel
    {
        public string Result { get; set; }
        public string Vendor { get; set; }
        public List<string> ChallanNoList { get; set; }
        public string BillNo { get; set; }
        public string BillAmount { get; set; }
        public string BillDate { get; set; }

        public string Company { get; set; }
        public string FullPaymentDone { get; set; }
        
    }
    public class BillModelForReport
    {
        public string Result { get; set; }
        public string Vendor { get; set; }
        public string ChallanNo { get; set; }
        public string BillNo { get; set; }
        public string BillAmount { get; set; }
        public string BillDate { get; set; }

        public string Company { get; set; }
        public string FullPaymentDone { get; set; }
    }
}

