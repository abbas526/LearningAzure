using ClosedXML.Excel;
using OrientalApplication.Core;
using OrientalApplication.DAL;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OrientalApplication.Controllers
{
    [CustomAuthorize(Roles = "Admin,Engineering")]
    public class ProjectMasterController : Controller
    {
        string PRDate = string.Empty;
        string DateRequired = string.Empty;
        // GET: ProjectMaster
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetProject(string projectName)
        {
            var project = ProjectMasterDAL.GetProject(projectName);
            return Json(project, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveProject(ProjectMaster projectMaster)
        {
            try
            {
                projectMaster.Result = ProjectMasterDAL.SaveData(projectMaster);
                return Json(projectMaster, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                projectMaster.Result = "Error in Save: " + ex.Message;
                return Json(projectMaster, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetSearchValue(string search)
        {
            var projects = ProjectMasterDAL.GetProjectNames();
            projects = projects.ConvertAll(d => d.ToUpper());

            List<string> allsearch = projects.Where(x => x.StartsWith(search.ToUpper())).Select(x => x).ToList();
            
            //List<string> allsearch = projects.Where(x => x.StartsWith(search)).Select(x => x).ToList();
            return new JsonResult { Data = allsearch, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult CloneProject(ProjectMaster projectMaster)
        {
            try
            {
                projectMaster.Result = ProjectMasterDAL.CloneProject(projectMaster);
                return Json(projectMaster, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                projectMaster.Result = "Error in Creating Project Copy: " + ex.Message;
                return Json(projectMaster, JsonRequestBehavior.AllowGet);
            }
        }
    
        public ActionResult UploadPRFile()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;

                    //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                    //string filename = Path.GetFileName(Request.Files[i].FileName);  
                    var projectName = Request.Params[0];
                    HttpPostedFileBase file = files[0];
                    string fname;
                    fname = file.FileName;
                    var proj = ProjectMasterDAL.GetProject(projectName);
                    if(proj == null)
                    {
                        return Json("Project not found for adding PRs. Please select a valid project name.");
                    }
                    // Get the complete folder path and store the file inside it.  
                    fname = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/FilesStore"),fname);// Path.Combine(Server.MapPath("~/Uploads/"), fname);
                    file.SaveAs(fname);

                    //Process Excel File and Save data in DB
                    int x = SavePRs(fname, projectName);

                    // Returns message that successfully uploaded  
                    return Json("Uploaded " + x + " Records");
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("PRdate: " + PRDate);
                    sb.AppendLine();
                    sb.Append("DateRequired: " + DateRequired);
                    sb.AppendLine();
                    sb.Append("Error in UploadPRFile Method : " + ex.Message);
                    sb.AppendLine("--StackTrace--");
                    sb.Append(ex.StackTrace);

                    return Json(sb.ToString());
                    //throw ex;
                    //return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        public JsonResult GetAllProjectNames()
        {
            var projectNamesList = ProjectMasterDAL.GetProjectNames();

            return new JsonResult { Data = projectNamesList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private int SavePRs(string fileName, string projectName)
        {

            int x = 1;
            int count = 0;
            using (var excelWorkbook = new XLWorkbook(fileName))
            {
                try
                {
                    var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
                    
                    foreach (var dataRow in nonEmptyDataRows)
                    {
                        if (x > 1)
                        {
                            //System.IO.File.WriteAllText(Server.MapPath("~/App_Data/LogFile1.txt"), "Inside line 138");
                            var pr = ConvertExcelRowToPR(dataRow);
                            pr.ProjectRefDropdown = projectName;

                            //// In case a new Item is coming from Excel file
                            var item = pr.ItemDropdown?.Trim().ToUpper();
                            var itemNames = ItemDAL.GetItemNames();
                            if (itemNames.Exists(y => y.ToUpper() == item) == false)
                            {
                                pr.NewItem = pr.ItemDropdown.Trim();
                            }
                            /////////
                            DateRequired = pr.DateRequired;
                            PRDate = pr.PRDate;
                            PurchaseRequisitionDAL.SavePR(pr);
                            count++;
                        }
                        x++;
                        
                    }
                }
                catch (Exception ex)
                {
                    throw ex; 
                }
            }            
            return count;
        }

        private static PurchaseRequisition ConvertExcelRowToPR(IXLRow prRow)
        {
            if (prRow != null)
            {
                PurchaseRequisition pr = new PurchaseRequisition();
                string PRDateDay = prRow.Cell((int)PRColumnNo.PRDateDay).Value?.ToString().Trim();
                if (PRDateDay.Length == 1)
                {
                    PRDateDay = "0" + PRDateDay;
                }

                string PRDateMonth = prRow.Cell((int)PRColumnNo.PRDateMonth).Value?.ToString().Trim();
                if(PRDateMonth.Length == 1)
                {
                    PRDateMonth = "0" + PRDateMonth;
                }

                string PRDateYear = prRow.Cell((int)PRColumnNo.PRDateYear).Value?.ToString().Trim();

                pr.PRDate = PRDateDay + "-" + PRDateMonth + "-" + PRDateYear;  

                //pr.PRDate = Convert.ToDateTime(pr.PRDate).ToString("dd-MM-yyyy"); //prRow.Cell((int)PRColumnNo.PRDate).Value?.ToString();
                //pr.PRNo = prRow.Cell((int)PRColumnNo.PRNo)?.ToString();
                //pr.ProjectRefDropdown = prRow.Cell((int)PRColumnNo.ProjectRef).Value?.ToString();
                pr.ItemDropdown = prRow.Cell((int)PRColumnNo.Item).Value?.ToString().Trim();
                //pr.NewItem = prRow.Cell((int)PRColumnNo.NewItem)?.ToString();
                pr.ItemSize = prRow.Cell((int)PRColumnNo.Size).Value?.ToString().Trim();
                pr.Specs = prRow.Cell((int)PRColumnNo.Specs).Value?.ToString().Trim();
                pr.Quantity = prRow.Cell((int)PRColumnNo.Qty).Value?.ToString().Trim();

                string DateReqdDay = prRow.Cell((int)PRColumnNo.DateReqdDay).Value?.ToString().Trim();
                if(DateReqdDay.Length == 1)
                {
                    DateReqdDay = "0" + DateReqdDay;
                }
                string DateReqdMonth = prRow.Cell((int)PRColumnNo.DateReqdMonth).Value?.ToString().Trim();
                if(DateReqdMonth.Length ==1)
                {
                    DateReqdMonth = "0" + DateReqdMonth;
                }
                string DateReqdYear = prRow.Cell((int)PRColumnNo.DateReqdYear).Value?.ToString().Trim();
                
                pr.DateRequired = DateReqdDay + "-" + DateReqdMonth + "-" + DateReqdYear;
                //pr.DateRequired = Convert.ToDateTime(pr.DateRequired).ToString("dd-MM-yyyy"); //prRow.Cell((int)PRColumnNo.DateReqd).Value?.ToString();                
                
                pr.Remark = prRow.Cell((int)PRColumnNo.Remark).Value?.ToString().Trim();
                pr.Unit = prRow.Cell((int)PRColumnNo.Unit).Value?.ToString().Trim();
                pr.UserCode = prRow.Cell((int)PRColumnNo.UserCode).Value?.ToString().Trim();
                pr.Drawing = prRow.Cell((int)PRColumnNo.Drawing).Value?.ToString().Trim();

                return pr;
            }
            return null;
        }

        public FileResult GetSampleExcel()
        {
            return File("~/App_Data/FilesStore/PRListTemplate.xlsx", "application/octet-stream", "PRListTemplate.xlsx");
        }

        

    }
}