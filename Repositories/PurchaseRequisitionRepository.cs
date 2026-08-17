using Dapper;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    public class PurchaseRequisitionRepository : IPurchaseRequisitionRepository
    {
        private readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        private readonly IItemRepository _itemRepository;

        public PurchaseRequisitionRepository() : this(new ItemRepository())
        {
        }

        public PurchaseRequisitionRepository(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public PurchaseRequisition GetPurchaseRequisition(string PRNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select * from PurchaseRequisition where prno=@PRNo and IsActive='yes'",
                    new { PRNo });

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }

                var itemReceivedRows = conn.Query(
                    "select * from PRItemReceived where prno=@PRNo",
                    new { PRNo });

                List<ItemReceived> itemReceivedList = new List<ItemReceived>();
                foreach (var row in itemReceivedRows)
                {
                    itemReceivedList.Add(ConvertObjectToItemReceived(row));
                }
                if (itemReceivedList.Count > 0)
                {
                    prList[0].ItemReceivedList = itemReceivedList;
                }
                return prList.FirstOrDefault();
            }
        }

        public PurchaseRequisition GetPurchaseRequisitionForDelete(string PRNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select * from PurchaseRequisition where prno=@PRNo and IsActive='yes' and PRNo not in (select PRNumber from PurchaseOrderItem) ",
                    new { PRNo });

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList.FirstOrDefault();
            }
        }

        public string GetLastPRNo()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                List<string> prNumbers = conn.Query<string>("select PRNo from PurchaseRequisition order by rowid desc limit 1").ToList();
                return prNumbers.LastOrDefault();
            }
        }

        private static PurchaseRequisition ConvertObjectToPR(dynamic row)
        {
            if (row == null)
            {
                return null;
            }

            var rowDict = (IDictionary<string, object>)row;

            PurchaseRequisition pr = new PurchaseRequisition();
            //pr.ItemReceivedObj = new ItemReceived();
            pr.PRDate = Convert.ToDateTime(row.PRDate).ToString("dd-MM-yyyy");
            pr.PRNo = Convert.ToString(row.PRNo);
            pr.ProjectRefDropdown = Convert.ToString(row.ProjectName);
            pr.ItemDropdown = Convert.ToString(row.ItemName);
            pr.ItemSize = Convert.ToString(row.Size);
            pr.Specs = Convert.ToString(row.Specs);
            pr.Quantity = Convert.ToString(row.Quantity);
            pr.DateRequired = SafeFormatDate(row.DateRequired);
            pr.Remark = Convert.ToString(row.Remark);
            pr.Drawing = Convert.ToString(row.Drawing);
            pr.Unit = Convert.ToString(row.UnitOfMeasurement);
            pr.UserCode = Convert.ToString(row.UserCode);
            if (rowDict.ContainsKey("PONumber"))
            {
                pr.AssignedToPO = Convert.ToString(row.PONumber);
            }
            pr.IsActive = Convert.ToString(row.IsActive);
            pr.IsAllItemReceived = Convert.ToString(row.IsAllItemReceived);
            return pr;
        }

        private static ItemReceived ConvertObjectToItemReceived(dynamic row)
        {
            ItemReceived itemReceived = new ItemReceived();
            itemReceived.ItemReceivedDate = SafeFormatDate(row.ReceivedDate);
            itemReceived.ItemReceivedOK = Convert.ToString(row.Condition);
            itemReceived.ItemReceivedComment = Convert.ToString(row.Comment);
            itemReceived.Vendor = Convert.ToString(row.Vendor);
            itemReceived.ItemReceivedChallanNo = Convert.ToString(row.ChallanNo);

            return itemReceived;
        }

        // Mirrors the try/catch date parsing the original reader-based code did inline: a
        // null/DBNull or unparseable date is left as null rather than thrown.
        private static string SafeFormatDate(object rawDate)
        {
            if (rawDate == null || rawDate is DBNull)
            {
                return null;
            }
            try
            {
                return Convert.ToDateTime(rawDate).ToString("dd-MM-yyyy");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string SavePR(PurchaseRequisition pr)
        {

            if (pr.DateRequired.Contains("/"))
            {
                var DateRequiredArray = pr.DateRequired.Split('/');
                pr.DateRequired = DateRequiredArray[2] + "-" + DateRequiredArray[1] + "-" + DateRequiredArray[0];

                var PRDateArray = pr.PRDate.Split('/');
                pr.PRDate = PRDateArray[2] + "-" + PRDateArray[1] + "-" + PRDateArray[0];
            }
            if (pr.DateRequired.Contains("-"))
            {
                var DateRequiredArray = pr.DateRequired.Split('-');
                pr.DateRequired = DateRequiredArray[2] + "-" + DateRequiredArray[1] + "-" + DateRequiredArray[0];


                var PRDateArray = pr.PRDate.Split('-');
                pr.PRDate = PRDateArray[2] + "-" + PRDateArray[1] + "-" + PRDateArray[0];
            }
            if (!string.IsNullOrEmpty(pr.PRNo))
            {
                return UpdatePR(pr);
            }
            else
            {
                var prNo = GetLastPRNo();
                int prNoNumber = 100;
                if (!string.IsNullOrEmpty(prNo) && prNo.Contains("-"))
                {
                    prNoNumber = Convert.ToInt32(prNo.Split('-')[1]) + 1;
                }
                pr.PRNo = pr.UserCode + "-" + prNoNumber;

                if (!string.IsNullOrEmpty(pr.NewItem))
                {
                    SaveNewItem(pr.NewItem.Trim());
                }

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    conn.Execute(
                        "INSERT INTO PurchaseRequisition(" +
                        "PRNo, " +
                        "PRDate," +
                        "ProjectName, " +
                        "ItemName, " +
                        "Size, " +
                        "Specs," +
                        "Quantity," +
                        "DateRequired," +
                        "Remark," +
                        "UserCode," +
                        "UnitOfMeasurement," +
                        "InsertedOn, " +
                        "IsActive, " +
                        "Drawing) " +
                        "VALUES(@PRNo,@PRDate,@ProjectName,@ItemName,@Size,@Specs,@Quantity,@DateRequired,@Remark,@UserCode,@UnitOfMeasurement,@InsertedOn,@IsActive,@Drawing)",
                        new
                        {
                            pr.PRNo,
                            pr.PRDate,
                            ProjectName = pr.ProjectRefDropdown,
                            ItemName = pr.ItemDropdown,
                            Size = pr.ItemSize,
                            pr.Specs,
                            pr.Quantity,
                            pr.DateRequired,
                            pr.Remark,
                            pr.UserCode,
                            UnitOfMeasurement = pr.Unit,
                            InsertedOn = DateTime.Now.ToString(),
                            IsActive = "yes",
                            pr.Drawing
                        });
                    return "Added Successfully";
                }
            }
        }

        private string UpdatePR(PurchaseRequisition pr)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                int rowsaffected = conn.Execute(
                    "UPDATE PurchaseRequisition " +
                    "SET PRDate=@PRDate" +
                    ",ProjectName = @ProjectName" +
                    ",ItemName = @ItemName" +
                    ",Size = @Size" +
                    ",Specs = @Specs" +
                    ",Quantity=@Quantity" +
                    ",DateRequired=@DateRequired" +
                    ",Remark=@Remark" +
                    ",UserCode=@UserCode" +
                    ",UnitOfMeasurement=@UnitOfMeasurement" +
                    ",UpdatedOn = @UpdatedOn" +
                    ",Drawing = @Drawing" +
                    " where PRNo=@PRNo ",
                    new
                    {
                        pr.PRDate,
                        ProjectName = pr.ProjectRefDropdown,
                        ItemName = pr.ItemDropdown,
                        Size = pr.ItemSize,
                        pr.Specs,
                        pr.Quantity,
                        pr.DateRequired,
                        pr.Remark,
                        pr.UserCode,
                        UnitOfMeasurement = pr.Unit,
                        UpdatedOn = DateTime.Now.ToString(),
                        pr.Drawing,
                        pr.PRNo
                    });
                if (rowsaffected == 0)
                {
                    return "Record Not Found";
                }
                else
                {
                    return "Updated Successfully";
                }
            }

        }

        private void SaveNewItem(string newItem)
        {
            //If Item Already exists then dont save
            var itemNames = _itemRepository.GetItemNames();
            if (itemNames.Exists(x => x.ToLower() == newItem.ToLower()))
            {
                return;
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute("INSERT INTO ItemMaster(Name) VALUES(@Name)", new { Name = newItem });
            }
        }

        public List<string> GetAllPurchaseRequisitionNumbers()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.Query<string>("select PRNo from PurchaseRequisition where IsActive='yes'").ToList();
            }
        }

        public List<PurchaseRequisition> GetPRsForProject(string project)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select * from PurchaseRequisition where IsActive='yes' and upper(trim(projectname))=@Project  " +
                    " and PRNo not in (select PRNumber from PurchaseOrderItem)",
                    new { Project = project.Trim().ToUpper() });

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList;
            }
        }

        public List<PurchaseRequisition> GetPendingPRs()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select * from PurchaseRequisition  where IsActive='yes' and " +
                    " PRNo not in (select PRNumber from PurchaseOrderItem) order by PRDate Desc");

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList;
            }
        }
        public List<PurchaseRequisition> GetAllPRsForProject(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select IFNULL(po.PONumber,'Not Mapped') as PONumber,pr.* " +
                    " from PurchaseRequisition pr left join PurchaseOrderItem po " +
                    "on pr.PRNo = po.PRNumber where upper(trim(pr.projectname))=@ProjectName",
                    new { ProjectName = projectName.Trim().ToUpper() });

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList;
            }
        }

        public List<PurchaseRequisition> GetPRsForPO(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select pr.* from PurchaseOrderItem po" +
                    " join PurchaseRequisition pr on po.PRNumber = pr.PRNo" +
                    " where pr.IsActive='yes' and po.PONumber = @PONumber",
                    new { PONumber = poNumber });

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList;
            }
        }

        public List<PurchaseRequisition> GetPRs(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select * from PurchaseRequisition where IsActive='yes' and upper(trim(projectname))=@ProjectName",
                    new { ProjectName = projectName.Trim().ToUpper() });

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList;
            }
        }

        public List<PurchaseRequisition> GetPRsWithPOStatus(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select IFNULL(po.PONumber,'Not Mapped') as PONumber,pr.* " +
                    " from PurchaseRequisition pr left join PurchaseOrderItem po " +
                    " on pr.PRNo = po.PRNumber where IsActive='yes' and upper(trim(projectname))=@ProjectName",
                    new { ProjectName = projectName.Trim().ToUpper() });

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList;
            }
        }

        public List<PurchaseRequisition> GetPRs()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query("select * from PurchaseRequisition");

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList;
            }
        }

        public List<PurchaseRequisition> GetPRs(string PRStartDate, string PREndDate)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string cmdText = "select IFNULL(po.PONumber,'Not Mapped') as PONumber,pr.* " +
                                   " from PurchaseRequisition pr left join PurchaseOrderItem po " +
                                   " on pr.PRNo = po.PRNumber ";
                object parameters = null;

                if (!string.IsNullOrEmpty(PRStartDate) && !string.IsNullOrEmpty(PREndDate))
                {
                    cmdText += " where PRDate >= @PRStartDate and PRDate <= @PREndDate";
                    parameters = new { PRStartDate, PREndDate };
                }
                else if (!string.IsNullOrEmpty(PRStartDate) && string.IsNullOrEmpty(PREndDate))
                {
                    cmdText += " where PRDate >= @PRStartDate";
                    parameters = new { PRStartDate };
                }

                var rows = conn.Query(cmdText, parameters);

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }
                return prList;
            }
        }

        public bool RemovePR(string prNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute(
                    "UPDATE PurchaseRequisition SET " +
                    " IsActive='no'" +
                    " WHERE PRNo=@PRNo and PRNo not in (select PRNumber from PurchaseOrderItem) ",
                    new { PRNo = prNumber });
                return true;
            }
        }

        public void DeleteItemReceived(string prNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute("DELETE FROM PRItemReceived WHERE PRNo = @PRNo", new { PRNo = prNo });
            }
        }

        public void UpdateAllItemReceivedFlag(string IsAllItemReceived, string prNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute(
                    "UPDATE PurchaseRequisition SET IsAllItemReceived = @IsAllItemReceived where PRNo = @PRNo ",
                    new { IsAllItemReceived, PRNo = prNo });
            }
        }

        public string InsertItemReceived(ItemReceived itemReceived)
        {
            if (itemReceived.ItemReceivedDate.Contains("/"))
            {
                var ItemReceivedArray = itemReceived.ItemReceivedDate.Split('/');
                itemReceived.ItemReceivedDate = ItemReceivedArray[2] + "-" + ItemReceivedArray[1] + "-" + ItemReceivedArray[0];
            }
            if (itemReceived.ItemReceivedDate.Contains("-"))
            {
                var ItemReceivedArray = itemReceived.ItemReceivedDate.Split('-');
                itemReceived.ItemReceivedDate = ItemReceivedArray[2] + "-" + ItemReceivedArray[1] + "-" + ItemReceivedArray[0];
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute(
                    "INSERT INTO PRItemReceived(PRNo,ReceivedDate,Condition,Comment,ChallanNo,Vendor) " +
                    "VALUES(@PRNo,@ReceivedDate,@Condition,@Comment,@ChallanNo,@Vendor)",
                    new
                    {
                        PRNo = itemReceived.PRNumber,
                        ReceivedDate = itemReceived.ItemReceivedDate,
                        Condition = itemReceived.ItemReceivedOK,
                        Comment = itemReceived.ItemReceivedComment,
                        ChallanNo = itemReceived.ItemReceivedChallanNo,
                        itemReceived.Vendor
                    });
                return "Added Successfully";
            }
        }

        public List<string> GetAllPOsForPR(string prNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.Query<string>(
                    "select IFNULL(po.PONumber,'Not Mapped') as PONumber " +
                    " from PurchaseRequisition pr left join PurchaseOrderItem po " +
                    "on pr.PRNo = po.PRNumber where upper(trim(pr.PRNo))=@PRNo",
                    new { PRNo = prNo.Trim().ToUpper() }).ToList();
            }
        }
    }
}
