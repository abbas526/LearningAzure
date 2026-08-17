using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IPaymentRepository
    {
        bool SaveVendorPaymentsWithBill(PaymentViewModel paymentViewModel, bool IsNew = true);
        int GetLastPaymentId();
        bool SaveOnlyVendorBill(BillModel bills, bool IsNew = true);
        List<BillModelForReport> GetBillsForReport(string vendor);
        List<BillModel> GetPendingBillData(string vendor);
        BillModel GetOnlyBillData(string BillNo, string vendor);
        List<string> GetPendingChallanNumbers(string vendor);
        List<string> GetVendorsWithOutstanding();
        List<string> GetVendorsforDashboard();
        List<VendorPayments> GetVendorPayments(string vendor);
        List<VendorPayments> GetVendorPaymentsAll(string vendor);
        List<VendorPaymentSummary> GetPaymentSummary();
        VendorPaymentSummary GetTotalPaymentSummary();
        BillsAndPaymentModel GetBillData(string BillNo, string vendor);
    }
}
