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
    [CustomAuthorize(Roles = "Admin,Engineering")]
    public class VendorController : Controller
    {
        private readonly IVendorRepository _vendorRepository;

        public VendorController() : this(new VendorRepository())
        {
        }

        public VendorController(IVendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        // GET: Vendor
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetVendor(string vendorName)
        {
            var vendor = _vendorRepository.GetVendor(vendorName);
            return Json(vendor, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveVendor(Vendor vendor)
        {
            try
            {
                vendor.Result = _vendorRepository.SaveData(vendor);
                return Json(vendor, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                vendor.Result = "Error in Save: " + ex.Message;
                return Json(vendor, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetSearchValue(string search)
        {
            var vendorList = _vendorRepository.GetAllVendorNames();
            vendorList = vendorList.ConvertAll(d => d.ToUpper());

            List<string> allsearch = vendorList.Where(x => x.StartsWith(search.ToUpper())).Select(x => x).ToList();

            return new JsonResult { Data = allsearch, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    
        

    }
}
