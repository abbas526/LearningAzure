using ClosedXML.Excel;
using OrientalApplication.Core;
using OrientalApplication.Models;
using OrientalApplication.Repositories;
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
        private readonly IItemRepository _itemRepository;

        public ItemController() : this(new ItemRepository())
        {
        }

        public ItemController(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        // GET: Item
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult SaveItem(Item item)
        {
            try
            {
                var itemList = _itemRepository.GetItemNames();
                if (itemList.Exists(y=>y.ToUpper() == item.ItemName.Trim().ToUpper()))
                {
                    item.Result = "Error : Item Already Exists";
                    return Json(item, JsonRequestBehavior.AllowGet);
                }
                item.Result = _itemRepository.SaveData(item);
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