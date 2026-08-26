using Dapper;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
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

        // PRDate/DateRequired/InsertedOn/UpdatedOn/ItemReceivedDate are declared DATE/DATETIME in
        // the schema, so System.Data.SQLite auto-converts them to .NET DateTime the moment Dapper
        // reads the row -- before ConvertObjectToPR/SafeFormatDate ever runs. A single row with a
        // non-ISO date string (e.g. "5-27-2021" instead of "2021-05-27") then throws
        // FormatException out of conn.Query() itself, taking down the whole list. Casting to TEXT
        // here keeps them as plain strings so our own safe parsing below is what actually runs.
        private const string PRColumns =
            "PRNo, CAST(PRDate AS TEXT) AS PRDate, ProjectName, ItemName, Size, Specs, Quantity, " +
            "CAST(DateRequired AS TEXT) AS DateRequired, Remark, UserCode, UnitOfMeasurement, BillNo, Vendor, Rate, " +
            "CAST(InsertedOn AS TEXT) AS InsertedOn, CAST(UpdatedOn AS TEXT) AS UpdatedOn, IsActive, " +
            "CAST(ItemReceivedDate AS TEXT) AS ItemReceivedDate, ItemReceivedOK, ItemReceivedComment, " +
            "ItemReceivedChallanNo, IsAllItemReceived, Drawing";

        private const string PRColumnsPrefixed =
            "pr.PRNo, CAST(pr.PRDate AS TEXT) AS PRDate, pr.ProjectName, pr.ItemName, pr.Size, pr.Specs, pr.Quantity, " +
            "CAST(pr.DateRequired AS TEXT) AS DateRequired, pr.Remark, pr.UserCode, pr.UnitOfMeasurement, pr.BillNo, pr.Vendor, pr.Rate, " +
            "CAST(pr.InsertedOn AS TEXT) AS InsertedOn, CAST(pr.UpdatedOn AS TEXT) AS UpdatedOn, pr.IsActive, " +
            "CAST(pr.ItemReceivedDate AS TEXT) AS ItemReceivedDate, pr.ItemReceivedOK, pr.ItemReceivedComment, " +
            "pr.ItemReceivedChallanNo, pr.IsAllItemReceived, pr.Drawing";

        private const string PRItemReceivedColumns =
            "PRNo, CAST(ReceivedDate AS TEXT) AS ReceivedDate, Condition, Comment, ChallanNo, Vendor";

        public PurchaseRequisition GetPurchaseRequisition(string PRNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select " + PRColumns + " from PurchaseRequisition where prno=@PRNo and IsActive='yes'",
                    new { PRNo });

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                foreach (var row in rows)
                {
                    prList.Add(ConvertObjectToPR(row));
                }

                var itemReceivedRows = conn.Query(
                    "select " + PRItemReceivedColumns + " from PRItemReceived where prno=@PRNo",
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
                    "select " + PRColumns + " from PurchaseRequisition where prno=@PRNo and IsActive='yes' and PRNo not in (select PRNumber from PurchaseOrderItem) ",
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
            pr.PRDate = SafeFormatDate(row.PRDate);
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

        // Dates from the UI can arrive as dd-MM-yyyy, dd/MM/yyyy, or (already) yyyy-MM-dd
        // depending on which form/date-picker posted them. This used to be handled by splitting
        // on whichever separator was present and blindly reversing the token order -- but that
        // logic ran as two independent `if`s (not if/else-if), so a "/"-separated date got
        // flipped once to yyyy-MM-dd and then flipped AGAIN because the result now contained "-",
        // landing on dd-MM-yyyy instead -- inconsistent with every other row in the table.
        // DateTime.TryParseExact against the known formats replaces that: it validates the input
        // properly and always normalizes to the same ISO "yyyy-MM-dd" storage format regardless of
        // which format came in.
        private static readonly string[] KnownDateFormats = { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };

        private static string NormalizeDateForStorage(string rawDate)
        {
            if (string.IsNullOrWhiteSpace(rawDate))
            {
                return rawDate;
            }
            if (DateTime.TryParseExact(rawDate.Trim(), KnownDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return parsed.ToString("yyyy-MM-dd");
            }
            // Unrecognized format: leave it as-is rather than mangle it further. It'll still be
            // saved (matching the previous best-effort behavior) but won't silently flip formats.
            return rawDate;
        }

        public string SavePR(PurchaseRequisition pr)
        {
            pr.PRDate = NormalizeDateForStorage(pr.PRDate);
            pr.DateRequired = NormalizeDateForStorage(pr.DateRequired);

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
                    "select " + PRColumns + " from PurchaseRequisition where IsActive='yes' and upper(trim(projectname))=@Project  " +
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
                    "select " + PRColumns + " from PurchaseRequisition  where IsActive='yes' and " +
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
                    "select IFNULL(po.PONumber,'Not Mapped') as PONumber," + PRColumnsPrefixed +
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
                    "select " + PRColumnsPrefixed + " from PurchaseOrderItem po" +
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
                    "select " + PRColumns + " from PurchaseRequisition where IsActive='yes' and upper(trim(projectname))=@ProjectName",
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
                    "select IFNULL(po.PONumber,'Not Mapped') as PONumber," + PRColumnsPrefixed +
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

                var rows = conn.Query("select " + PRColumns + " from PurchaseRequisition");

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

                string cmdText = "select IFNULL(po.PONumber,'Not Mapped') as PONumber," + PRColumnsPrefixed +
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
            itemReceived.ItemReceivedDate = NormalizeDateForStorage(itemReceived.ItemReceivedDate);

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
