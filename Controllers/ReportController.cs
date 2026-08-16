using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using OrientalApplication.Core;
using OrientalApplication.DAL;
using OrientalApplication.Models;

namespace OrientalApplication.Controllers
{
    [CustomAuthorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult WritePRToExcel(string prStartDate = "", string prEndDate = "") 
        {
            
               DataTable dt = GetPRData(prStartDate, prEndDate);
                //Name of File
                //
                
                string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

                string fileName = "PRList-" + strDate  + ".xlsx";
                using (XLWorkbook wb = new XLWorkbook())
                {
                    //Add DataTable in worksheet  
                    wb.Worksheets.Add(dt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        //Return xlsx Excel File  
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
        }


        public ActionResult WritePOToExcel(string poStartDate = "", string poEndDate = "")
        {

            DataTable dt = GetPOData(poStartDate, poEndDate);
            //Name of File
            //

            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "POList-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public ActionResult WritePOItemToExcel(string poStartDate = "", string poEndDate = "")
        {

            DataTable dt = GetPOItemData(poStartDate, poEndDate);
            //Name of File
            //

            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "POItemList-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public ActionResult WritePOForProjectToExcel(string projectName)
        {

            projectName = projectName.Replace("||","&");

            DataTable dt = GetPODataForProject(projectName);
            //Name of File
            //

            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "POForProjectList-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public ActionResult WritePRsForPOItemToExcel(string poNumber)
        {

            DataTable dt = GetPRsForPO(poNumber);
            //Name of File
            //

            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "PRsForPOList-" + poNumber + "-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public ActionResult WritePRsForProjectItemToExcel(string projectName)
        {
            projectName = projectName.Replace("||", "&");

            DataTable dt = GetPRsForProject(projectName);
            //Name of File
            //

            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "PRsForProjectList-" + "-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public ActionResult WritePendingPRToExcel()
        {

            DataTable dt = GetPendingPR();
            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "PendingPRList-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public ActionResult WriteVendorBillsToExcel(string Vendor)
        {

                DataTable dt = GetBills(Vendor);
            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "BillsList-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public ActionResult WriteChallanToExcel(string projectName)
        {
            projectName = projectName.Replace("||", "&");
            DataTable dt = GetChallans(projectName);
            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "ChallansList-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        private DataTable GetPOData(string poStartDate, string poEndDate)
        {
            var poList = PurchaseOrderDAL.GetPurchaseOrder(poStartDate, poEndDate);
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(poList); //
            return dt;
        }

        private DataTable GetPODataForProject(string projectName)
        {
            projectName = projectName.Replace("||", "&");
            var poList = PurchaseOrderDAL.GetAllPurchaseOrder(projectName);
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(poList); //
            return dt;
        }

        private DataTable GetPOItemData(string poStartDate, string poEndDate)
        {
            var poItemsList = PurchaseOrderItemDAL.GetPurchaseOrderItems(poStartDate, poEndDate);
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(poItemsList); 
            return dt;
        }


        private DataTable GetPRData(string prStartDate, string prEndDate)
        {
            var prList = PurchaseRequisitionDAL.GetPRs(prStartDate,prEndDate);
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(prList); //
            return dt;
        }

        private DataTable GetPendingPR()
        {
            var prList = PurchaseRequisitionDAL.GetPendingPRs();
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(prList); //
            return dt;
        }

        private DataTable GetPRsForPO(string poNumber)
        {
            var prList = PurchaseRequisitionDAL.GetPRsForPO(poNumber);
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(prList); //
            return dt;
        }

        private DataTable GetPRsForProject(string projectName)
        {
            projectName = projectName.Replace("||", "&");

            var prList = PurchaseRequisitionDAL.GetAllPRsForProject(projectName);
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(prList); //
            return dt;
        }

        public ActionResult WriteVendorToExcel()
        {

            DataTable dt = GetVendors();
            //Name of File
            //

            string strDate = DateTime.Now.Date.ToString("dd-MM-yyyy");

            string fileName = "Vendors-" + strDate + ".xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        private DataTable GetVendors()
        {
            var vendors = VendorDAL.GetAllVendors();
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(vendors); //
            return dt;

        }

        private DataTable GetChallans(string projectName)
        {
            projectName = projectName.Replace("||", "&");
            var prList = OutgoingChallanDAL.GetAllChallans(projectName);
            //Creating DataTable
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(prList); //
            return dt;
        }

        public DataTable GetBills(string Vendor = null)
		{
            var bills = PaymentDAL.GetBillsForReport(Vendor);
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(bills); 
            return dt;
        }

        public ActionResult ValidatePassword(string pass)
        {
            if (pass == "2001")
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else 
            {
                return Json("Failure", JsonRequestBehavior.AllowGet);
            }        
        }

        public FileResult DownloadDB()
        {
            string filepath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), "OrientalDB.db");
            byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);
            string fileName = "OrientalDB.db";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }

    public class ListtoDataTableConverter

    {

        public DataTable ToDataTable<T>(List<T> items)

        {

            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)

            {

                //Setting column names as Property names

                dataTable.Columns.Add(prop.Name);

            }

            foreach (T item in items)

            {

                var values = new object[Props.Length];

                for (int i = 0; i < Props.Length; i++)

                {

                    //inserting property values to datatable rows

                    values[i] = Props[i].GetValue(item, null);

                }

                dataTable.Rows.Add(values);

            }

            //put a breakpoint here and check datatable

            return dataTable;

        }

    }
}


