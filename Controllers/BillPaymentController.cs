using OrientalApplication.Core;
using OrientalApplication.Models;
using OrientalApplication.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrientalApplication.Controllers
{
    [CustomAuthorize(Roles = "Accounts,Admin")]
    public class BillPaymentController : Controller
    {
        private readonly IPaymentRepository _paymentRepository;

        public BillPaymentController() : this(new PaymentRepository())
        {
        }

        public BillPaymentController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        // GET: BillPayment
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetChallanNumbers(string vendor)
        {
            List<string> challanNos = _paymentRepository.GetPendingChallanNumbers(vendor);
            if (challanNos != null && challanNos.Count > 0)
            {
                return Json(challanNos, JsonRequestBehavior.AllowGet);
            }
            else
			{
                return Json("Note: No Challan Numbers to create Bill", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult BillSave(BillModel billModel)
		{
            try
            {


                var billdata = _paymentRepository.GetOnlyBillData(billModel.BillNo, billModel.Vendor);

                var ch = billModel.ChallanNoList[0].Split(',');
                billModel.ChallanNoList = new List<string>();
                billModel.ChallanNoList.AddRange(ch);
                if (billdata != null)
                {
                    _paymentRepository.SaveOnlyVendorBill(billModel, false);
                }
                else
                {
                    _paymentRepository.SaveOnlyVendorBill(billModel, true);
                }

                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
			{
                return Json("Failure : Not able to Save. " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BillPaymentNewSave(PaymentViewModel model)
		{
            if (model.BillDetails == null || model.BillDetails.Count == 0)
			{
                return Json("Failure : No Bill Details were selected", JsonRequestBehavior.AllowGet);
            }
            try
            {
                _paymentRepository.SaveVendorPaymentsWithBill(model, true);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
			{
                return Json("Failure : " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetVendorNames()
        {
            var names = _paymentRepository.GetVendorsWithOutstanding();
            return Json(names, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVendorsForDashBoard()
        {
            var names = _paymentRepository.GetVendorsforDashboard();
            return Json(names, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBillData(string billNo, string vendor)
        {
            var data = _paymentRepository.GetOnlyBillData(billNo, vendor);
            if (data == null)
            {
                data = new BillModel();
                data.Result = "Error: No bill found for selected Bill number: " + billNo;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            data.Result = "Success";
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        // Load the data in the Bill data grid
        public ActionResult GetPendingBillData(string vendor)
		{
            var data = _paymentRepository.GetPendingBillData(vendor);
            if (data == null || data.Count==0)
            {              
                string result = "Note: No pending bill found for selected Vendor: " + vendor;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        //Used to fill the last 20 payments done
        public ActionResult GetPaymentDetails(string vendor)
        {
            var data = _paymentRepository.GetVendorPayments(vendor);
            
            return Json(data, JsonRequestBehavior.AllowGet);
        }

		#region Dashboard
		public ActionResult VendorPaymentDashBoard()
        {
            return View();
        }

        // Used in Dashboard
        public JsonResult GetPayments(string vendor, string year, string month)
        {
            var summaryList = _paymentRepository.GetPaymentSummary();
            if (!string.IsNullOrEmpty(vendor))
            {
                var list = summaryList.Where(x => x.Vendor == vendor)?.ToList();
                if(list != null && list.Count > 0)
                {
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(summaryList, JsonRequestBehavior.AllowGet);
        }

        // Used in Dashboard
        public JsonResult GetTotalPayments(string year, string month)
        {
            var summaryList = _paymentRepository.GetTotalPaymentSummary();
            return Json(summaryList, JsonRequestBehavior.AllowGet);
        }
		#endregion

	}
}

