using OrientalApplication.Core;
using OrientalApplication.Models;
using OrientalApplication.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrientalApplication.Controllers
{

    public class PRVM
    {
        public string PRNumber { get; set; }
    }
    [CustomAuthorize(Roles = "Admin,Engineering,")]
    public class PurchaseRequisitionController : Controller
    {
        private readonly IPurchaseRequisitionRepository _purchaseRequisitionRepository;
        private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;

        public PurchaseRequisitionController()
            : this(new PurchaseRequisitionRepository(), new PurchaseOrderItemRepository(), new ItemRepository(), new PurchaseOrderRepository())
        {
        }

        public PurchaseRequisitionController(
            IPurchaseRequisitionRepository purchaseRequisitionRepository,
            IPurchaseOrderItemRepository purchaseOrderItemRepository,
            IItemRepository itemRepository,
            IPurchaseOrderRepository purchaseOrderRepository)
        {
            _purchaseRequisitionRepository = purchaseRequisitionRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _itemRepository = itemRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        // GET: PurchaseRequisition

        public ActionResult PrintPR(PRVM prModel)
        {
            if (prModel == null || prModel.PRNumber == null)
            {
                return null;
            }


            PurchaseRequisition pr = _purchaseRequisitionRepository.GetPurchaseRequisition(prModel.PRNumber);

            if (pr == null)
            {
                Response.Write("Invalid Purchase Requisition Number.");
                Response.End();
            }
            else
            {
                var PRDate = pr.PRDate.Split(' ')[0].Split('-');
                var PRDateString = PRDate[2] + "-" + PRDate[1] + "-" + PRDate[0];

                var DateRequired = pr.DateRequired.Split(' ')[0].Split('-');
                var DateRequiredString = DateRequired[2] + "-" + DateRequired[1] + "-" + DateRequired[0];

                pr.PRDate = PRDateString;
                pr.DateRequired = DateRequiredString;

                return View(pr);
            }
            return null;
        }

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult PRGet(string PRNo)
        {
            PurchaseRequisition po = _purchaseRequisitionRepository.GetPurchaseRequisition(PRNo);
            
            List<String> PONumberList = _purchaseRequisitionRepository.GetAllPOsForPR(PRNo);
            string PONumbers = string.Join(",", PONumberList);
            po.AssociatedPONumbers = PONumbers;

            return Json(po, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPRs(string project)
        {
            var prList = _purchaseRequisitionRepository.GetPRsWithPOStatus(project);
            return Json(prList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PRSave(PurchaseRequisition pr)
        {
            try 
            {
                pr.IsNew = "true";
                if (!string.IsNullOrEmpty(pr.NewItem))
                {
                    pr.ItemDropdown = pr.NewItem.Replace("'","''");
                }
                if (!string.IsNullOrEmpty(pr.PRNo))
                {
                    var prFoundInPO = _purchaseOrderItemRepository.GetPurchaseOrderItem(pr.PRNo);
                    if (prFoundInPO != null && prFoundInPO.PONumber != null)
                    {
                        string msg = "Error: " + "The PR cannot be saved because it is already used in PO No: " + prFoundInPO.PONumber;
                        pr.Result = msg;
                        return Json(pr, JsonRequestBehavior.AllowGet);
                    }
                    pr.IsNew = "false";
                }
                var result = _purchaseRequisitionRepository.SavePR(pr);

                pr.Result = result;
                if (pr.ItemDropdown == null || pr.ItemDropdown == "")
                {
                    pr.ItemDropdown = pr.NewItem;
                }
            }
            catch (Exception ex)
            {                
                pr.Result = "Error : " + ex.Message;
            }
            return Json(pr, JsonRequestBehavior.AllowGet);
        }

        List<String> GetPRNumberList()
        {
            // every time we get a request, we will check the cache first
            List<string> purchaseRequisitionList = HttpContext.Cache.Get("PRNumberList") as List<string>;

            // if the cached object is null, then only fetch data from Database
            if (purchaseRequisitionList == null)
            {
                purchaseRequisitionList = _purchaseRequisitionRepository.GetAllPurchaseRequisitionNumbers();
                //after fetching from database,insert the collection into Cache object.
                HttpContext.Cache.Insert("PRNumberList", purchaseRequisitionList);
                
            }
            return purchaseRequisitionList;
        }

        public JsonResult GetSearchValue(string search)
        {
            var prNumberList = GetPRNumberList();
            prNumberList = prNumberList.ConvertAll(d => d.ToUpper());

            List<string> allsearch = prNumberList.Where(x => x.StartsWith(search.ToUpper())).Select(x => x).ToList();

            return new JsonResult { Data = allsearch, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetProjectNames()
        {
              var projectNamesList = Utility.GetProjectNames();

              return new JsonResult { Data = projectNamesList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetOldProjectNames()
        {
            var projectNamesList = Utility.GetOldProjectNames();
            return new JsonResult { Data = projectNamesList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult GetItemNames()
        {

            var itemNamesList = _itemRepository.GetItemNames();

            return new JsonResult { Data = itemNamesList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public bool CheckDuplicateItemName(string itemName)
        {
            var itemNamesList = _itemRepository.GetItemNames();
            return itemNamesList.Exists(x => x == itemName);
        }

        public JsonResult GeneratePRNo() {
            var prNo = _purchaseRequisitionRepository.GetLastPRNo();
            int prNoNumber = 100; 
            if (!string.IsNullOrEmpty(prNo) && prNo.Contains("-"))
            {
                prNoNumber = Convert.ToInt32(prNo.Split('-')[1]) + 1;
            }
            var t = new { PRNoNumber = prNoNumber};
            return new JsonResult { Data = t, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult RemovePR(string prNumber, string poAdminCode)
        {
            if (string.IsNullOrEmpty(poAdminCode) || poAdminCode.Trim() != "1234")
            {
                return Json("Error: InValid Admin Code, cannot remove PR", JsonRequestBehavior.AllowGet);
            }
            PurchaseRequisition pr = _purchaseRequisitionRepository.GetPurchaseRequisitionForDelete(prNumber);
            
            if (pr == null || string.IsNullOrEmpty(pr.PRNo))
            {
                return Json("Error: PR does not exist Or PR is already deleted Or PR is associated to a PO. Hence cannot Delete PR.", JsonRequestBehavior.AllowGet);
            }

            var result = _purchaseRequisitionRepository.RemovePR(prNumber);
            if (result == true)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult QuickPOSave(QuickPO quickPO)
        {
            //Check PR Number os valid
            PurchaseRequisition pr = _purchaseRequisitionRepository.GetPurchaseRequisition(quickPO.PRNumberHidden);
            if (pr == null)
            {
                pr = new PurchaseRequisition();
                pr.Result = "Error: PR Number is not valid";
                return Json(pr, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var prFoundInPO = _purchaseOrderItemRepository.GetPurchaseOrderItem(pr.PRNo);
                if (prFoundInPO != null && prFoundInPO.PONumber != null)
                {
                    string msg = "Error: " + "The PO cannot be saved because the PR is already used in PO No: " + prFoundInPO.PONumber;
                    pr.Result = msg;
                    return Json(pr, JsonRequestBehavior.AllowGet);
                }
            }

            try
            {
                string PONumber = "Q" + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0');
                PurchaseOrder po = new PurchaseOrder();
                po.PONumber = PONumber;
                po.PODate = DateTime.Now.Day.ToString().PadLeft(2, '0') + "-" +  DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Year.ToString().PadLeft(2, '0');
                po.Vendor = quickPO.VendorForPO;
                po.QuoteRef = "Not Applicable";
                po.HSNNo = "Not Applicable";               
                po.PORemarks = quickPO.PORemarks;
                po.DeliveryRequiredBy = "01-01-1990";
                po.DeliveryInstructions = "Not Applicable";
                po.DeliveryRequiredAt = "Not Applicable";
                po.TransportationCharges = "Not Applicable";
                po.PaymentTerms = "Not Applicable";
                po.Company = "Not Applicable";
                double amt = 0.00;
                if(!string.IsNullOrEmpty(quickPO.POQty))
                {
                    amt = Convert.ToDouble(quickPO.Rate) * Convert.ToDouble(quickPO.POQty);
                }
                else
                {
                    amt = Convert.ToDouble(quickPO.Rate) * Convert.ToDouble(pr.Quantity);
                }
                po.POAmount = amt.ToString();
                po.ProjectRef = pr.ProjectRefDropdown;
                po.AllProjects = pr.ProjectRefDropdown;
                po.BillAmount = "0";
                po.BillNoAndDate = "Not Applicable";
                po.PaymentDate = "";

                _purchaseOrderRepository.SaveData(po, true);

                POItemSave(quickPO,po.PONumber);
                pr.Result = "PO Saved Successfully, New PO Number : " + po.PONumber;
                return Json(pr, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var p = new PurchaseRequisition();
                p.Result = "Exception in QuickPOSave, Error: " + ex.Message;
                return Json(p, JsonRequestBehavior.AllowGet);
            }
        }
        public void POItemSave(QuickPO quickPO, string PONumber)
        {

            PurchaseOrderItem prItem = new PurchaseOrderItem();

            prItem.POQuantity = quickPO.POQty;
            prItem.PRNumber = quickPO.PRNumberHidden;
            prItem.Rate = quickPO.Rate;
            prItem.PONumber = PONumber;

            _purchaseOrderItemRepository.SaveData(prItem);
            
        }

        public ActionResult ItemReceivedSave(List<ItemReceived> itemReceivedList, string IsAllItemReceived)
        {

            _purchaseRequisitionRepository.DeleteItemReceived(itemReceivedList[0].PRNumber);

            foreach (var item in itemReceivedList)
            {
                _purchaseRequisitionRepository.InsertItemReceived(item);
            }

            _purchaseRequisitionRepository.UpdateAllItemReceivedFlag(IsAllItemReceived, itemReceivedList[0].PRNumber);

            return Json("Success", JsonRequestBehavior.AllowGet);            
        }

    }
}