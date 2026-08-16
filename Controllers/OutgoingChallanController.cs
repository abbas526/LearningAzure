using OrientalApplication.Core;
using OrientalApplication.DAL;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OrientalApplication.Controllers
{
    [CustomAuthorize(Roles = "Admin,Engineering")]
    public class OutgoingChallanController : Controller
    {
        // GET: OutgoingChallan
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChallanSave(OutgoingChallan po)
        {
            try
            {
                var p = OutgoingChallanDAL.GetOutgoingChallan(po.ChallanNumber);

                if (p != null && !string.IsNullOrEmpty(p.ChallanNumber))
                {
                    OutgoingChallanDAL.SaveChallan(po, false);
                    //return Json("PO Number Already Exists", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    OutgoingChallanDAL.SaveChallan(po, true);
                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult ChallanItemSave( List<OutgoingChallanItem> challanItems)
        {
            foreach (var item in challanItems)
            {
                OutgoingChallanItemDAL.SaveData(item);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVendorNames()
        {
            var names = VendorDAL.GetVendorNames("Challan");
            return Json(names, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVendorGST(string vendorName)
        {
            var GST = VendorDAL.GetVendorGST(vendorName);
            return Json(GST, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCompanyNames()
        {
            var companies = CompanyDAL.GetCompanies();
            //companies.RemoveAt(0);
            List<string> companyList = new List<string>();
            companyList = companies.Select(x => x.CompanyName + "==" + x.ContactPerson).ToList();
            return Json(companyList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChallan(string challanNumber)
        {
            var data = OutgoingChallanDAL.GetOutgoingChallan(challanNumber);
            if (data == null)
            {
                data = new OutgoingChallan();
                data.Result = "Error : Challan Number Not Found";
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            data.VendorGST = VendorDAL.GetVendorGST(data.Vendor);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PrintChallan(string challanNumber)
        {
            if (string.IsNullOrEmpty(challanNumber))
            {
                return null;
            }


            OutgoingChallan challan = OutgoingChallanDAL.GetOutgoingChallan(challanNumber);

            if (challan == null)
            {
                Response.Write("Invalid Challan Number.");
                Response.End();
            }
            else
            {              

                var companies = CompanyDAL.GetCompanies();
                POCompany company = new POCompany();
                foreach (var c in companies)
                {
                    string f = c.CompanyName + "==" + c.ContactPerson;
                    if (f == challan.Company)
                    {
                        company = c; break;
                    }
                }
                string challanDateString;
                var challanDate = challan.ChallanDate.Split('-');                    
                challanDateString = challanDate[0] + "-" + challanDate[1] + "-" + challanDate[2];
                challan.ChallanDate = challanDateString;

                Vendor vendor = null;
                if (!string.IsNullOrEmpty(challan.Vendor))
                {
                    var vendors = VendorDAL.GetVendors("Challan");
                    vendor = vendors.FirstOrDefault(x => x.VendorName.Contains(challan.Vendor));
                }
                ChallanPrintViewModel model = new ChallanPrintViewModel();
                model.Challan = challan;

                model.Challan = challan;
                model.POCompany = company;
                model.POVendor = vendor;

                model.Challan.FinalAmount = challan.OutgoingChallanItems.Sum(x => Convert.ToDouble(x.Amount)).ToString();

                return View(model);
            }
            return null;
        }

        public JsonResult GenerateChallanNo(string Company)
        {
            string Abbr = "";
            if(Company.ToLower().Contains("mechanical"))
            {
                Abbr = "M";
            }
            else
            {
                Abbr = "F";
            }
            var challanNo = OutgoingChallanDAL.GetLastChallanNo(Abbr);
            int challanNoNumber = 0; //1001;
            if (challanNo.Contains("-"))
            {
                var arr = challanNo.Split('-');
                challanNoNumber =  Convert.ToInt32(arr[1]) + 1;
            }

            //if (!string.IsNullOrEmpty(challanNo))
            //{
            //    challanNoNumber = Convert.ToInt32(challanNo) + 1;
            //}
            challanNo = Abbr + "-" + challanNoNumber;
            var t = new { ChallanNumber = challanNo };
            return new JsonResult { Data = t, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

    }
}