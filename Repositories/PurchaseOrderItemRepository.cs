using Dapper;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    public class PurchaseOrderItemRepository : IPurchaseOrderItemRepository
    {
        private readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public bool SaveData(PurchaseOrderItem poi)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute(
                    "INSERT INTO PurchaseOrderItem(PONumber, PRNumber,Rate, POQuantity, Discount) " +
                    "VALUES(@PONumber,@PRNumber,@Rate,@POQuantity,@Discount)",
                    poi);
                return true;
            }
        }

        public List<PurchaseOrderItem> GetPurchaseOrderItems(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select po.*,pr.unitOfMeasurement," +
                    "pr.ProjectName,pr.ItemName,pr.Quantity,pr.Size,CAST(por.PODate AS TEXT) AS PODate,pr.Drawing from PurchaseOrderItem po " +
                    "join PurchaseRequisition pr on po.PRNumber = pr.PRNo " +
                    "join PurchaseOrder por on po.PONumber = por.PONumber " +
                    "where por.IsActive='yes' and  po.PONumber=@PONumber",
                    new { PONumber = poNumber });

                List<PurchaseOrderItem> poItemList = new List<PurchaseOrderItem>();
                foreach (var row in rows)
                {
                    poItemList.Add(ConvertObjectToPOItem(row));
                }
                return poItemList;
            }
        }

        public PurchaseOrderItem GetPurchaseOrderItem(string prNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.QueryFirstOrDefault<PurchaseOrderItem>(
                    "select * from PurchaseOrderItem where PRNumber=@PRNumber",
                    new { PRNumber = prNumber });
            }
        }

        public List<PurchaseOrderItem> GetPurchaseOrderItems()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query("select * from PurchaseOrderItem");

                List<PurchaseOrderItem> poItemList = new List<PurchaseOrderItem>();
                foreach (var row in rows)
                {
                    poItemList.Add(ConvertObjectToPOItem(row));
                }
                return poItemList;
            }
        }

        public List<PurchaseOrderItem> GetPurchaseOrderItems(string poStartDate, string poEndDate)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string sql = "select po.*,pr.unitOfMeasurement," +
                                                "pr.ProjectName,pr.ItemName,pr.Quantity,pr.Size,CAST(por.PODate AS TEXT) AS PODate from PurchaseOrderItem po " +
                                                "join PurchaseRequisition pr on po.PRNumber = pr.PRNo " +
                                                "join PurchaseOrder por on po.PONumber = por.PONumber ";
                object parameters = null;

                if (!string.IsNullOrEmpty(poStartDate) && !string.IsNullOrEmpty(poEndDate))
                {
                    sql += "where por.PODate >= @PoStartDate and por.PODate <= @PoEndDate";
                    parameters = new { PoStartDate = poStartDate, PoEndDate = poEndDate };
                }
                else if (!string.IsNullOrEmpty(poStartDate) && string.IsNullOrEmpty(poEndDate))
                {
                    sql += "where por.PODate >= @PoStartDate";
                    parameters = new { PoStartDate = poStartDate };
                }

                var rows = conn.Query(sql, parameters);

                List<PurchaseOrderItem> poList = new List<PurchaseOrderItem>();
                foreach (var row in rows)
                {
                    PurchaseOrderItem po = new PurchaseOrderItem();
                    po = ConvertObjectToPOItem(row);
                    poList.Add(po);
                }
                return poList;
            }
        }

        private static PurchaseOrderItem ConvertObjectToPOItem(dynamic row)
        {
            PurchaseOrderItem poItem = new PurchaseOrderItem();
            poItem.PONumber = Convert.ToString(row.PONumber);
            poItem.PRNumber = Convert.ToString(row.PRNumber);
            poItem.Rate = Convert.ToString(row.Rate);
            poItem.Discount = Convert.ToString(row.Discount);
            poItem.POQuantity = Convert.ToString(row.POQuantity);
            poItem.Unit = Convert.ToString(row.unitOfMeasurement);
            poItem.ProjectName = Convert.ToString(row.ProjectName);
            poItem.ItemName = Convert.ToString(row.ItemName);
            poItem.Qty = Convert.ToString(row.Quantity);
            poItem.ItemSize = Convert.ToString(row.Size);
            poItem.Drawing = Convert.ToString(row.Drawing);
            poItem.PODate = Convert.ToString(row.PODate);

            return poItem;
        }
    }
}
