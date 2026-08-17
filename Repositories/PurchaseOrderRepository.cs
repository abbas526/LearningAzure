using Dapper;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

        public PurchaseOrder GetPurchaseOrder(string PONo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var row = conn.QueryFirstOrDefault(
                    "select * from PurchaseOrder where IsActive='yes' and PONumber=@PONumber",
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
                             "pr.Quantity, pr.ItemName, pr.UnitOfMeasurement, po.PODate, po.IsActive, po.Vendor " +
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
            po.PODate = Convert.ToDateTime(row.PODate).ToString("dd-MM-yyyy");
            po.Vendor = Convert.ToString(row.Vendor);
            po.QuoteRef = Convert.ToString(row.QuoteRef);
            po.HSNNo = Convert.ToString(row.HSN);
            po.PORemarks = Convert.ToString(row.PORemarks);
            po.DeliveryRequiredBy = Convert.ToDateTime(row.DeliveryRequiredBy).ToString("dd-MM-yyyy");
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
            // Original wrapped this in a try/catch, but a plain ToString() on a DBNull/string
            // column can't actually throw, so there's nothing for the catch to do.
            po.PaymentDate = Convert.ToString(row.PaymentDate);
            return po;
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
            po.PODate = Convert.ToDateTime(row.PODate).ToString("dd-MM-yyyy");
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
                    sql = "SELECT * FROM PurchaseOrder WHERE PODate >= @StartDate AND PODate <= @EndDate";
                    parameters = new { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") };
                }
                else if (DateTime.TryParse(poStartDate, out startDate) && !DateTime.TryParse(poEndDate, out _))
                {
                    sql = "SELECT * FROM PurchaseOrder WHERE PODate >= @StartDate";
                    parameters = new { StartDate = startDate.ToString("yyyy-MM-dd") };
                }
                else
                {
                    sql = "select * from PurchaseOrder";
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

                if (po.PODate.Contains("/"))
                {
                    var PODateArray = po.PODate.Split('/');
                    po.PODate = PODateArray[2] + "-" + PODateArray[1] + "-" + PODateArray[0];

                    var DeliveryRequiredByArray = po.DeliveryRequiredBy.Split('/');
                    po.DeliveryRequiredBy = DeliveryRequiredByArray[2] + "-" + DeliveryRequiredByArray[1] + "-" + DeliveryRequiredByArray[0];

                    if (!string.IsNullOrEmpty(po.PaymentDate))
                    {
                        var PaymentDateArray = po.PaymentDate.Split('/');
                        po.PaymentDate = PaymentDateArray[2] + "-" + PaymentDateArray[1] + "-" + PaymentDateArray[0];
                    }
                }

                if (po.PODate.Contains("-"))
                {
                    var PODateArray = po.PODate.Split('-');
                    po.PODate = PODateArray[2] + "-" + PODateArray[1] + "-" + PODateArray[0];

                    var DeliveryRequiredByArray = po.DeliveryRequiredBy.Split('-');
                    po.DeliveryRequiredBy = DeliveryRequiredByArray[2] + "-" + DeliveryRequiredByArray[1] + "-" + DeliveryRequiredByArray[0];

                    if (!string.IsNullOrEmpty(po.PaymentDate))
                    {
                        var PaymentDateArray = po.PaymentDate.Split('-');
                        po.PaymentDate = PaymentDateArray[2] + "-" + PaymentDateArray[1] + "-" + PaymentDateArray[0];
                    }
                }

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
