using OrientalApplication.Core;
using OrientalApplication.DAL;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Xceed.Document.NET;
using Xceed.Words.NET;


namespace OrientalApplication.Controllers
{
    public class POVM
    {
        public string PONumber { get; set; }
    }
    [CustomAuthorize(Roles = "Admin,Engineering,")]
    public class PurchaseOrderController : Controller
    {

        public ActionResult PrintPO(POVM poModel) {
            if (poModel == null || poModel.PONumber == null)
            {
                return null;
            }

            
            PurchaseOrder po = PurchaseOrderDAL.GetPurchaseOrder(poModel.PONumber);

            if (po == null)
            {
                Response.Write("Invalid Purchase Order Number.");
                Response.End();
            }
            else
            {
                var poItemsVM = GetPOItemsVM(poModel.PONumber);

                var companies = CompanyDAL.GetCompanies();
                POCompany company = new POCompany();
                foreach (var c in companies)
                {
                    string f = c.CompanyName + "==" + c.ContactPerson;
                    if (f == po.Company)
                    {
                        company = c; break;
                    }
                }
                string PODateString;
                if (po.PODate.Contains(" "))
                {
                    var PODate = po.PODate.Split(' ')[0].Split('-');
                    PODateString = PODate[0] + "-" + PODate[1] + "-" + PODate[2];
                }
                else {
                    var PODate = po.PODate.Split('-');
                    PODateString = PODate[0] + "-" + PODate[1] + "-" + PODate[2];
                }

                string DeliveryRequiredByString;
                if (po.DeliveryRequiredBy.Contains(" "))
                {
                    var DeliveryRequiredBy = po.DeliveryRequiredBy.Split(' ')[0].Split('-');
                    DeliveryRequiredByString = DeliveryRequiredBy[0] + "-" + DeliveryRequiredBy[1] + "-" + DeliveryRequiredBy[2];
                }
                else
                {
                    var DeliveryRequiredBy = po.DeliveryRequiredBy.Split('-');
                    DeliveryRequiredByString = DeliveryRequiredBy[0] + "-" + DeliveryRequiredBy[1] + "-" + DeliveryRequiredBy[2];

                }

                po.PODate = PODateString;
                po.DeliveryRequiredBy = DeliveryRequiredByString;
                Vendor vendor = null;
                if (!string.IsNullOrEmpty(po.Vendor))
                {
                    var vendors = VendorDAL.GetVendors("PO");
                    vendor = vendors.FirstOrDefault(x => x.VendorName.Contains(po.Vendor));
                }
                POPrintViewModel model = new POPrintViewModel();
                model.PO = po;

                model.POItemsVM = poItemsVM;
                model.POCompany = company;
                model.POVendor = vendor;
                if (!string.IsNullOrEmpty(model.PO.DeliveryRequiredAt))
                {
                    if (model.PO.DeliveryRequiredAt.ToLower() == "taloja" || model.PO.DeliveryRequiredAt.ToLower() == "taloja factory")
                    {
                        model.PO.DeliveryRequiredAt = "A 15 MIDC, Taloja, Near Pendhar Village, Panvel 410208";
                    }
                    else if (model.PO.DeliveryRequiredAt.ToLower() == "rabale" || model.PO.DeliveryRequiredAt.ToLower() == "rabale factory")
                    {
                        model.PO.DeliveryRequiredAt = "R 271 , T.T.C. Industrial Area, Thane Belapur Road, Rabale, Navi Mumbai 400701";
                    }
                }
                return View(model);
            }
            return null;
        }

        private List<POItemsViewModel> GetPOItemsVM(string poNumber)
        {
            var poItems = PurchaseOrderItemDAL.GetPurchaseOrderItems(poNumber);
            var prList = PurchaseRequisitionDAL.GetPRsForPO(poNumber);

            var finalPOItems = new List<POItemsViewModel>();
            int i = 1;
            foreach (var po in poItems)
            {
                var f = new POItemsViewModel();
                var pr = prList.Where(x => x.PRNo == po.PRNumber).First();                
                var p = poItems.Where(x => x.PRNumber == po.PRNumber).First();
                f.SrNo = i.ToString();
                i++;
                f.Particulars = pr.ItemDropdown;
                f.Size = pr.ItemSize;
                f.MOC = pr.Specs;
                f.Rate = p.Rate;
                f.Qty = (string.IsNullOrEmpty(p.POQuantity) || p.POQuantity == "0") ? pr.Quantity : p.POQuantity;
                f.Discount = po.Discount;
                f.Unit = pr.Unit;
                f.PRNo = po.PRNumber;
                finalPOItems.Add(f);
            }
            return finalPOItems;
        }

        // GET: PurchaseOrder
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult POGet(string PONumber)
        {
            
            PurchaseOrder po = PurchaseOrderDAL.GetPurchaseOrder(PONumber);

            if (po == null)
            {
                return Json("Not Found", JsonRequestBehavior.AllowGet);
            }
            return Json(po, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult POSave(PurchaseOrder po)
        {
            try
            {
                var p = PurchaseOrderDAL.GetPurchaseOrder(po.PONumber);

                po.DisplayDiscount = (!string.IsNullOrEmpty(po.DisplayDiscount)) ? "true" : "false";
                po.DisplayTotal = (!string.IsNullOrEmpty(po.DisplayTotal)) ? "true" : "false";

                //Calculate PO Total
                if (string.IsNullOrEmpty(po.POAmount))
                {
                    return Json("Calculate PO Amount and then Save", JsonRequestBehavior.AllowGet);
                }

                if (p != null && string.IsNullOrEmpty(p.PONumber) == false)
                {
                    PurchaseOrderDAL.SaveData(po, false);
                }
                else
                {
                    PurchaseOrderDAL.SaveData(po, true);
                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult POItemSave(List<PurchaseOrderItem> purchaseOrderItems)
        {
            foreach (var item in purchaseOrderItems)
            {                
                PurchaseOrderItemDAL.SaveData(item);
            }            
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVendorNames()
        {            
            var names = VendorDAL.GetVendorNames();
            return Json(names, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCompanyNames()
        {            
            var companies = CompanyDAL.GetCompanies();
            List<string> companyList = null;
            companyList = companies.Select(x => x.CompanyName + "==" + x.ContactPerson).ToList();
            return Json(companyList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPRs(string project)
        {
            var prList = PurchaseRequisitionDAL.GetPRsForProject(project);
            return Json(prList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPaymentTerms()
        {
            var paymentTerms = Utility.GetPaymentTerms();                                   
            return Json(paymentTerms, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProjectNames(SearchViewModel model)
        {
            var projectList = Utility.GetProjectNames();

            List<string> ar = model.projects != null ? model.projects.ToList() : null;
            if (ar != null && ar.Count() > 0)
            {
                projectList.RemoveAll(item => ar.Contains(item));
            }


            return new JsonResult { Data = projectList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpGet]    
        public JsonResult RemovePO(string poNumber, string poAdminCode)
        {
            if (string.IsNullOrEmpty(poAdminCode) || poAdminCode.Trim() != "1234")
            {
                return Json("Error: InValid PO Admin Code, cannot remove PO", JsonRequestBehavior.AllowGet);
            }

            PurchaseOrder po = PurchaseOrderDAL.GetPurchaseOrder(poNumber);
            if(po == null || string.IsNullOrEmpty(po.PONumber))
            {
                return Json("Error: PO does not exist", JsonRequestBehavior.AllowGet);
            }
            var result = PurchaseOrderDAL.RemovePO(poNumber);
            if (result == true)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ApprovePO(string poNumber, string poApproverCode)
        {
            if (string.IsNullOrEmpty(poApproverCode) || poApproverCode.Trim() != "1990")
            {
                return Json("Error: InValid PO Approver Code, cannot approve PO", JsonRequestBehavior.AllowGet);
            }

            PurchaseOrder po = PurchaseOrderDAL.GetPurchaseOrder(poNumber);
            if (po == null || string.IsNullOrEmpty(po.PONumber))
            {
                return Json("Error: PO does not exist", JsonRequestBehavior.AllowGet);
            }
            
            if (po.IsApprovalRequested != "yes")
            {
                return Json("Error: PO has not been sent for Approval", JsonRequestBehavior.AllowGet);
            }
            var result = PurchaseOrderDAL.ApprovePO(poNumber);
            if (result == true)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SubmitForApproval(string poNumber)
        {

            PurchaseOrder po = PurchaseOrderDAL.GetPurchaseOrder(poNumber);
            
            if (po == null || string.IsNullOrEmpty(po.PONumber))
            {
                return Json("Error: PO does not exist", JsonRequestBehavior.AllowGet);
            }
            if (po.IsApproved == "yes")
            {
                return Json("Error : Already Approved", JsonRequestBehavior.AllowGet);
            }
            if (po.IsApprovalRequested == "yes")
            {
                 return Json("Error : Already Requested Approval", JsonRequestBehavior.AllowGet);
            }
            PurchaseOrderDAL.SubmitForApproval(poNumber);
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
    }
}
