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
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;

        public PurchaseOrderRepository() : this(new PurchaseOrderItemRepository())
        {
        }

        public PurchaseOrderRepository(IPurchaseOrderItemRepository purchaseOrderItemRepository)
        {
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
        }

        // PODate/DeliveryRequiredBy/InsertedOn/UpdatedOn/PaymentDate are declared DATE/DATETIME in
        // the schema, so System.Data.SQLite auto-converts them to .NET DateTime while Dapper
        // materializes the row -- before our own try/catch parsing below ever runs. A malformed
        // stored value then throws FormatException out of conn.Query() itself. Casting to TEXT
        // keeps them as plain strings so ConvertObject/SafeFormatDate do the (safe) parsing
        // instead. See PurchaseRequisitionRepository.cs for the full write-up of this quirk.
        private const string PurchaseOrderColumns =
            "PONumber, CAST(PODate AS TEXT) AS PODate, Vendor, QuoteRef, HSN, PORemarks, " +
            "CAST(DeliveryRequiredBy AS TEXT) AS DeliveryRequiredBy, DeliveryInstructions, DeliveryRequiredAt, " +
            "TransportationCharges, PaymentTerms, Company, POAmount, AllProjects, BillAmount, BillNoAndDate, " +
            "CAST(InsertedOn AS TEXT) AS InsertedOn, CAST(UpdatedOn AS TEXT) AS UpdatedOn, IsActive, ProjectRef, " +
            "CAST(PaymentDate AS TEXT) AS PaymentDate, DisplayTotal, DisplayDiscount, IsApproved, IsApprovalRequested";

        public PurchaseOrder GetPurchaseOrder(string PONo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var row = conn.QueryFirstOrDefault(
                    "select " + PurchaseOrderColumns + " from PurchaseOrder where IsActive='yes' and PONumber=@PONumber",
                    new { PONumber = PONo });

                if (row != null)
                {
                    var po = ConvertObject(row);

                    var purchaseOrderItems = _purchaseOrderItemRepository.GetPurchaseOrderItems(PONo);

                    po.PurchaseOrderItems = purchaseOrderItems;

                    return po;
                }
                return null;
            }
        }

        public List<PurchaseOrderDetails> GetAllPurchaseOrder(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string sql = "SELECT pr.ProjectName, po.PONumber, poItem.PRNumber, poItem.Rate, poItem.Discount, poItem.POQuantity, " +
                             "pr.Quantity, pr.ItemName, pr.UnitOfMeasurement, CAST(po.PODate AS TEXT) AS PODate, po.IsActive, po.Vendor " +
                             "FROM PurchaseOrder po " +
                             "join PurchaseOrderItem poItem on po.PONumber = poItem.PONumber " +
                             "join PurchaseRequisition pr on poItem.PRNumber = pr.PRNo " +
                             "where pr.ProjectName = @ProjectName";

                var rows = conn.Query(sql, new { ProjectName = projectName });

                var purchaseOrders = new List<PurchaseOrderDetails>();
                foreach (var row in rows)
                {
                    purchaseOrders.Add(ConvertObjectToPODetails(row));
                }
                return purchaseOrders;
            }
        }


        private static PurchaseOrder ConvertObject(dynamic row)
        {
            PurchaseOrder po = new PurchaseOrder();
            po.PONumber = Convert.ToString(row.PONumber);
            po.PODate = SafeFormatDate(row.PODate);
            po.Vendor = Convert.ToString(row.Vendor);
            po.QuoteRef = Convert.ToString(row.QuoteRef);
            po.HSNNo = Convert.ToString(row.HSN);
            po.PORemarks = Convert.ToString(row.PORemarks);
            po.DeliveryRequiredBy = SafeFormatDate(row.DeliveryRequiredBy);
            po.DeliveryInstructions = Convert.ToString(row.DeliveryInstructions);
            po.DeliveryRequiredAt = Convert.ToString(row.DeliveryRequiredAt);
            po.TransportationCharges = Convert.ToString(row.TransportationCharges);
            po.PaymentTerms = Convert.ToString(row.PaymentTerms);
            po.Company = Convert.ToString(row.Company);
            po.POAmount = Convert.ToString(row.POAmount);
            po.ProjectRef = Convert.ToString(row.ProjectRef);
            po.AllProjects = Convert.ToString(row.AllProjects);
            po.IsActive = Convert.ToString(row.IsActive);
            po.BillAmount = Convert.ToString(row.BillAmount);
            po.BillNoAndDate = Convert.ToString(row.BillNoAndDate);
            po.DisplayTotal = Convert.ToString(row.DisplayTotal);
            po.DisplayDiscount = Convert.ToString(row.DisplayDiscount);
            po.IsApproved = Convert.ToString(row.IsApproved);
            po.IsApprovalRequested = Convert.ToString(row.IsApprovalRequested);
            // PaymentDate is declared DATE and is nullable (a PO may not be paid yet), so it goes
            // through the same safe parsing as the other date columns rather than a bare
            // Convert.ToString/ToDateTime.
            po.PaymentDate = SafeFormatDate(row.PaymentDate);
            return po;
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

        // See PurchaseRequisitionRepository.NormalizeDateForStorage for why this replaces the
        // previous separator-sniffing split/reorder logic: DateTime.TryParseExact against the
        // known incoming formats always normalizes to ISO "yyyy-MM-dd" instead of risking a
        // format flip.
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
            return rawDate;
        }

        private static PurchaseOrderDetails ConvertObjectToPODetails(dynamic row)
        {
            PurchaseOrderDetails po = new PurchaseOrderDetails();

            po.ProjectName = Convert.ToString(row.ProjectName);
            po.PONumber = Convert.ToString(row.PONumber);
            po.PRNumber = Convert.ToString(row.PRNumber);
            po.Rate = Convert.ToString(row.Rate);
            po.Discount = Convert.ToString(row.Discount);
            po.POQuantity = Convert.ToString(row.POQuantity);
            po.PRQty = Convert.ToString(row.Quantity);
            po.ItemName = Convert.ToString(row.ItemName);
            po.Unit = Convert.ToString(row.UnitOfMeasurement);
            po.PODate = SafeFormatDate(row.PODate);
            po.IsActive = Convert.ToString(row.IsActive);
            po.Vendor = Convert.ToString(row.Vendor);
            return po;
        }

        public List<PurchaseOrder> GetPurchaseOrder(string poStartDate, string poEndDate)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                DateTime startDate, endDate;
                string sql;
                object parameters = null;

                if (DateTime.TryParse(poStartDate, out startDate) && DateTime.TryParse(poEndDate, out endDate))
                {
                    sql = "SELECT " + PurchaseOrderColumns + " FROM PurchaseOrder WHERE PODate >= @StartDate AND PODate <= @EndDate";
                    parameters = new { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") };
                }
                else if (DateTime.TryParse(poStartDate, out startDate) && !DateTime.TryParse(poEndDate, out _))
                {
                    sql = "SELECT " + PurchaseOrderColumns + " FROM PurchaseOrder WHERE PODate >= @StartDate";
                    parameters = new { StartDate = startDate.ToString("yyyy-MM-dd") };
                }
                else
                {
                    sql = "select " + PurchaseOrderColumns + " from PurchaseOrder";
                }

                var rows = conn.Query(sql, parameters);

                List<PurchaseOrder> poList = new List<PurchaseOrder>();
                foreach (var row in rows)
                {
                    PurchaseOrder po = new PurchaseOrder();
                    po = ConvertObject(row);
                    poList.Add(po);
                }
                return poList;
            }
        }

        public bool SaveData(PurchaseOrder po, bool IsNew = true)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                po.PODate = NormalizeDateForStorage(po.PODate);
                po.DeliveryRequiredBy = NormalizeDateForStorage(po.DeliveryRequiredBy);
                po.PaymentDate = NormalizeDateForStorage(po.PaymentDate);

                int res;
                if (IsNew)
                {
                    res = conn.Execute(
                        "INSERT INTO PurchaseOrder" +
                        "(PONumber" +
                        ",PODate" +
                        ",Vendor" +
                        ",QuoteRef" +
                        ",HSN" +
                        ",PORemarks" +
                        ",DeliveryRequiredBy" +
                        ",DeliveryInstructions" +
                        ",DeliveryRequiredAt" +
                        ",TransportationCharges" +
                        ",PaymentTerms" +
                        ",Company" +
                        ",POAmount" +
                        ",ProjectRef" +
                        ",AllProjects" +
                        ",InsertedOn" +
                        ",IsActive" +
                        ",BillAmount" +
                        ",BillNoAndDate" +
                        ",PaymentDate" +
                        ",DisplayTotal" +
                        ",DisplayDiscount" +
                        ") " +
                        "VALUES(@PONumber,@PODate,@Vendor,@QuoteRef,@HSN,@PORemarks,@DeliveryRequiredBy,@DeliveryInstructions,@DeliveryRequiredAt,@TransportationCharges,@PaymentTerms,@Company,@POAmount,@ProjectRef,@AllProjects,@InsertedOn,@IsActive,@BillAmount,@BillNoAndDate,@PaymentDate,@DisplayTotal,@DisplayDiscount)",
                        new
                        {
                            po.PONumber,
                            po.PODate,
                            po.Vendor,
                            po.QuoteRef,
                            HSN = po.HSNNo,
                            po.PORemarks,
                            po.DeliveryRequiredBy,
                            po.DeliveryInstructions,
                            po.DeliveryRequiredAt,
                            po.TransportationCharges,
                            po.PaymentTerms,
                            po.Company,
                            po.POAmount,
                            po.ProjectRef,
                            po.AllProjects,
                            InsertedOn = DateTime.Now.ToString(),
                            IsActive = "yes",
                            po.BillAmount,
                            po.BillNoAndDate,
                            po.PaymentDate,
                            po.DisplayTotal,
                            po.DisplayDiscount
                        });
                }
                else
                {
                    res = conn.Execute(
                        "UPDATE PurchaseOrder SET " +
                        " PODate=@PODate" +
                        ",Vendor=@Vendor" +
                        ",QuoteRef=@QuoteRef" +
                        ",HSN=@HSN" +
                        ",PORemarks=@PORemarks" +
                        ",DeliveryRequiredBy=@DeliveryRequiredBy" +
                        ",DeliveryInstructions=@DeliveryInstructions" +
                        ",DeliveryRequiredAt=@DeliveryRequiredAt" +
                        ",TransportationCharges=@TransportationCharges" +
                        ",PaymentTerms=@PaymentTerms" +
                        ",Company=@Company" +
                        ",POAmount=@POAmount" +
                        ",ProjectRef=@ProjectRef" +
                        ",AllProjects=@AllProjects" +
                        ",UpdatedOn=@UpdatedOn" +
                        ",BillAmount=@BillAmount" +
                        ",BillNoAndDate=@BillNoAndDate" +
                        ",PaymentDate=@PaymentDate" +
                        ",DisplayTotal=@DisplayTotal" +
                        ",DisplayDiscount=@DisplayDiscount" +
                        " WHERE PONumber=@PONumber ",
                        new
                        {
                            po.PODate,
                            po.Vendor,
                            po.QuoteRef,
                            HSN = po.HSNNo,
                            po.PORemarks,
                            po.DeliveryRequiredBy,
                            po.DeliveryInstructions,
                            po.DeliveryRequiredAt,
                            po.TransportationCharges,
                            po.PaymentTerms,
                            po.Company,
                            po.POAmount,
                            po.ProjectRef,
                            po.AllProjects,
                            UpdatedOn = DateTime.Now.ToString(),
                            po.BillAmount,
                            po.BillNoAndDate,
                            po.PaymentDate,
                            po.DisplayTotal,
                            po.DisplayDiscount,
                            po.PONumber
                        });
                }
                if (res > 0)
                {
                    //POItems delete - the entire set will be inserted again as a new entry for a PO Update
                    conn.Execute("DELETE FROM PurchaseOrderItem WHERE PONumber = @PONumber", new { po.PONumber });
                }
                return true;
            }
        }

        public bool RemovePO(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var res = conn.Execute(
                    "UPDATE PurchaseOrder SET IsActive='no' WHERE PONumber=@PONumber ",
                    new { PONumber = poNumber });

                if (res > 0)
                {
                    //POItems delete -
                    conn.Execute("DELETE FROM PurchaseOrderItem WHERE PONumber = @PONumber", new { PONumber = poNumber });
                }
                return true;
            }
        }

        public bool ApprovePO(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute(
                    "UPDATE PurchaseOrder SET IsApproved='yes' WHERE PONumber=@PONumber ",
                    new { PONumber = poNumber });
                return true;
            }
        }

        public bool SubmitForApproval(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute(
                    "UPDATE PurchaseOrder SET IsApprovalRequested='yes' WHERE PONumber=@PONumber ",
                    new { PONumber = poNumber });
                return true;
            }
        }
    }
}
