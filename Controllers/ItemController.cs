using ClosedXML.Excel;
using OrientalApplication.Core;
using OrientalApplication.DAL;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OrientalApplication.Controllers
{
    [CustomAuthorize(Roles = "Admin,Engineering")]
    public class ItemController : Controller
    {
        
        // GET: Item
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult SaveItem(Item item)
        {
            try
            {
                var itemList = ItemDAL.GetItemNames();
                if (itemList.Exists(y=>y.ToUpper() == item.ItemName.Trim().ToUpper()))
                {
                    item.Result = "Error : Item Already Exists";
                    return Json(item, JsonRequestBehavior.AllowGet);
                }
                item.Result = ItemDAL.SaveData(item);
                return Json(item, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                item.Result = "Error in Save: " + ex.Message;
                return Json(item, JsonRequestBehavior.AllowGet);
            }
        }
    }
}