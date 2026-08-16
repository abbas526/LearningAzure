using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Web;

namespace OrientalApplication.DAL
{
    public class PurchaseOrderItemDAL
    {
        private static readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public static bool SaveData(PurchaseOrderItem poi)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(conn);
                    string insertStatement = string.Format("INSERT INTO PurchaseOrderItem(PONumber, PRNumber,Rate, POQuantity, Discount) " +
                        "VALUES('{0}','{1}','{2}','{3}','{4}')", poi.PONumber, poi.PRNumber, poi.Rate, poi.POQuantity, poi.Discount);
                    cmd.CommandText = insertStatement;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }   
        }

        public static List<PurchaseOrderItem> GetPurchaseOrderItems(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    SQLiteCommand cmd = new SQLiteCommand(conn);
                    cmd.CommandText = string.Format("select po.*,pr.unitOfMeasurement," +
                                                    "pr.ProjectName,pr.ItemName,pr.Quantity,pr.Size,por.PODate,pr.Drawing from PurchaseOrderItem po " +
                                                    "join PurchaseRequisition pr on po.PRNumber = pr.PRNo " +
                                                    "join PurchaseOrder por on po.PONumber = por.PONumber " +
                                                    "where por.IsActive='yes' and  po.PONumber='{0}'", poNumber);
                    var reader = cmd.ExecuteReader();

                    List<PurchaseOrderItem> poItemList = new List<PurchaseOrderItem>();
                    while (reader.Read())
                    {
                        poItemList.Add(ConvertObjectToPOItem(reader));
                    }
                    conn.Close();
                    return poItemList;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static PurchaseOrderItem GetPurchaseOrderItem(string prNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = string.Format("select * from PurchaseOrderItem" +
                                                " where PRNumber='{0}'", prNumber);
                var reader = cmd.ExecuteReader();

                List<PurchaseOrderItem> poItemList = new List<PurchaseOrderItem>();
                while (reader.Read())
                {
                    poItemList.Add(ConvertObjectToPOItemOnly(reader));
                }
                conn.Close();
                if (poItemList == null || poItemList.Count == 0)
                {
                    return null;
                }
                
                return poItemList[0];
            }
        }

        public static List<PurchaseOrderItem> GetPurchaseOrderItems()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from PurchaseOrderItem";
                var reader = cmd.ExecuteReader();

                List<PurchaseOrderItem> poItemList = new List<PurchaseOrderItem>();
                while (reader.Read())
                {
                    poItemList.Add(ConvertObjectToPOItem(reader));
                }
                conn.Close();
                return poItemList;

            }
        }

        internal static List<PurchaseOrderItem> GetPurchaseOrderItems(string poStartDate, string poEndDate)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);

                if (!string.IsNullOrEmpty(poStartDate) && !string.IsNullOrEmpty(poEndDate))
                {
                    cmd.CommandText = "select po.*,pr.unitOfMeasurement," +
                                                    "pr.ProjectName,pr.ItemName,pr.Quantity,pr.Size,por.PODate from PurchaseOrderItem po " +
                                                    "join PurchaseRequisition pr on po.PRNumber = pr.PRNo " +
                                                    "join PurchaseOrder por on po.PONumber = por.PONumber " +
                                                    "where por.PODate >= '" + poStartDate + "' and por.PODate <= '" + poEndDate + "'";
                }
                else if (!string.IsNullOrEmpty(poStartDate) && string.IsNullOrEmpty(poEndDate))
                {
                    cmd.CommandText = "select po.*,pr.unitOfMeasurement," +
                                                    "pr.ProjectName,pr.ItemName,pr.Quantity,pr.Size,por.PODate from PurchaseOrderItem po " +
                                                    "join PurchaseRequisition pr on po.PRNumber = pr.PRNo " +
                                                    "join PurchaseOrder por on po.PONumber = por.PONumber " +
                                                    "where por.PODate >= '" + poStartDate + "'";

                }
                else
                {
                    cmd.CommandText = "select po.*,pr.unitOfMeasurement," +
                                                    "pr.ProjectName,pr.ItemName,pr.Quantity,pr.Size,por.PODate from PurchaseOrderItem po " +
                                                    "join PurchaseRequisition pr on po.PRNumber = pr.PRNo " +
                                                    "join PurchaseOrder por on po.PONumber = por.PONumber ";


                }
                var reader = cmd.ExecuteReader();

                List<PurchaseOrderItem> poList = new List<PurchaseOrderItem>();
                while (reader.Read())
                {
                    PurchaseOrderItem po = new PurchaseOrderItem();
                    po = ConvertObjectToPOItem(reader);
                    poList.Add(po);
                }
                conn.Close();
                return poList;
            }
        }

        private static PurchaseOrderItem ConvertObjectToPOItem(SQLiteDataReader reader)
        {
            PurchaseOrderItem poItem = new PurchaseOrderItem();
            poItem.PONumber = reader["PONumber"].ToString();
            poItem.PRNumber = reader["PRNumber"].ToString();
            poItem.Rate = reader["Rate"].ToString();
            poItem.Discount = reader["Discount"].ToString();
            poItem.POQuantity = reader["POQuantity"].ToString();
            poItem.Unit = reader["unitOfMeasurement"].ToString();
            poItem.ProjectName = reader["ProjectName"].ToString();
            poItem.ItemName = reader["ItemName"].ToString();
            poItem.Qty = reader["Quantity"].ToString();
            poItem.ItemSize = reader["Size"].ToString();
            poItem.Drawing = reader["Drawing"].ToString();
            poItem.PODate = reader["PODate"].ToString();
            
            return poItem;
        }

        private static PurchaseOrderItem ConvertObjectToPOItemOnly(SQLiteDataReader reader)
        {
            PurchaseOrderItem poItem = new PurchaseOrderItem();
            poItem.PONumber = reader["PONumber"].ToString();
            poItem.PRNumber = reader["PRNumber"].ToString();
            poItem.Rate = reader["Rate"].ToString();
            poItem.Discount = reader["Discount"].ToString();
            poItem.POQuantity = reader["POQuantity"].ToString();
            return poItem;
        }

    }
}