using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Web;

namespace OrientalApplication.DAL
{
    public static class PurchaseOrderDAL
    {
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        public static PurchaseOrder GetPurchaseOrder(string PONo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from PurchaseOrder where IsActive='yes' and PONumber='" + PONo + "'";

                var reader = cmd.ExecuteReader();

                var purchaseOrders = new List<PurchaseOrder>();
                while (reader.Read())
                {
                    purchaseOrders.Add(ConvertObject(reader));
                }
                conn.Close();
                if (purchaseOrders.Count() > 0)
                {
                    var po = purchaseOrders.First();

                    var purchaseOrderItems = PurchaseOrderItemDAL.GetPurchaseOrderItems(PONo);

                    po.PurchaseOrderItems = purchaseOrderItems;

                    return po;
                }
                return null;
            }
        }

        public static List<PurchaseOrderDetails> GetAllPurchaseOrder(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);

                StringBuilder sb = new StringBuilder("SELECT");
                sb.Append(" pr.ProjectName,");
                sb.Append(" po.PONumber,");
                sb.Append(" poItem.PRNumber,");
                sb.Append(" poItem.Rate,");
                sb.Append(" poItem.Discount,");
                sb.Append(" poItem.POQuantity,");
                sb.Append(" pr.Quantity,");
                sb.Append(" pr.ItemName,");
                sb.Append(" pr.UnitOfMeasurement,");
                sb.Append(" po.PODate,");
                sb.Append(" po.IsActive,");
                sb.Append(" po.Vendor");
                sb.Append(" FROM PurchaseOrder po");
                sb.Append(" join PurchaseOrderItem poItem on po.PONumber = poItem.PONumber");
                sb.Append(" join PurchaseRequisition pr on poItem.PRNumber = pr.PRNo");
                sb.Append(" where pr.ProjectName = '" + projectName + "'");
                cmd.CommandText = sb.ToString();                    

                var reader = cmd.ExecuteReader();

                var purchaseOrders = new List<PurchaseOrderDetails>();
                while (reader.Read())
                {
                    purchaseOrders.Add(ConvertObjectToPODetails(reader));
                }
                conn.Close();
                return purchaseOrders;
            }
        }


        private static PurchaseOrder ConvertObject(SQLiteDataReader reader)
        {


            PurchaseOrder po = new PurchaseOrder();                
            po.PONumber = reader["PONumber"]?.ToString();
            po.PODate = Convert.ToDateTime(reader["PODate"]).ToString("dd-MM-yyyy"); //reader["PODate"]?.ToString(); //
            po.Vendor = reader["Vendor"]?.ToString();
            po.QuoteRef = reader["QuoteRef"]?.ToString();
            po.HSNNo = reader["HSN"]?.ToString();
            po.PORemarks = reader["PORemarks"]?.ToString();
            po.DeliveryRequiredBy = Convert.ToDateTime(reader["DeliveryRequiredBy"]).ToString("dd-MM-yyyy");
            po.DeliveryInstructions = reader["DeliveryInstructions"]?.ToString();
            po.DeliveryRequiredAt = reader["DeliveryRequiredAt"]?.ToString();
            po.TransportationCharges = reader["TransportationCharges"]?.ToString();
            po.PaymentTerms = reader["PaymentTerms"]?.ToString();
            po.Company = reader["Company"]?.ToString();            
            po.POAmount = reader["POAmount"]?.ToString();
            po.ProjectRef = reader["ProjectRef"]?.ToString();
            po.AllProjects = reader["AllProjects"]?.ToString();
            po.IsActive = reader["IsActive"]?.ToString();
            po.BillAmount = reader["BillAmount"]?.ToString();
            po.BillNoAndDate = reader["BillNoAndDate"]?.ToString();
            po.DisplayTotal = reader["DisplayTotal"]?.ToString();
            po.DisplayDiscount = reader["DisplayDiscount"]?.ToString();
            po.IsApproved = reader["IsApproved"]?.ToString();
            po.IsApprovalRequested  = reader["IsApprovalRequested"]?.ToString();
            try
            {
                po.PaymentDate = reader["PaymentDate"]?.ToString();
            }
            catch (Exception)
            { 
                
            }
            return po;
        }

        private static PurchaseOrderDetails ConvertObjectToPODetails(SQLiteDataReader reader)
        {
            PurchaseOrderDetails po = new PurchaseOrderDetails();

            po.ProjectName = reader["ProjectName"]?.ToString();
            po.PONumber = reader["PONumber"]?.ToString();
            po.PRNumber = reader["PRNumber"]?.ToString();
            po.Rate = reader["Rate"]?.ToString();
            po.Discount = reader["Discount"]?.ToString();
            po.POQuantity = reader["POQuantity"]?.ToString();
            po.PRQty = reader["Quantity"]?.ToString();
            po.ItemName = reader["ItemName"]?.ToString();
            po.Unit = reader["UnitOfMeasurement"]?.ToString();
            po.PODate = Convert.ToDateTime(reader["PODate"]).ToString("dd-MM-yyyy"); 
            po.IsActive = reader["IsActive"]?.ToString();
            po.Vendor = reader["Vendor"]?.ToString();
            return po;
        }

        internal static List<PurchaseOrder> GetPurchaseOrder(string poStartDate, string poEndDate)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                DateTime startDate, endDate;
                //if (!string.IsNullOrEmpty(poStartDate) && !string.IsNullOrEmpty(poEndDate))
                //{
                if (DateTime.TryParse(poStartDate, out startDate) && DateTime.TryParse(poEndDate, out endDate))
                {
                    //cmd.CommandText = "select * from PurchaseOrder where PODate >= '" + poStartDate + "' and PODate <= '" + poEndDate + "'";
                    // Convert the dates to yyyy-MM-dd format
                    string startDateStr = startDate.ToString("yyyy-MM-dd");
                    string endDateStr = endDate.ToString("yyyy-MM-dd");

                    cmd.CommandText = "SELECT * FROM PurchaseOrder WHERE PODate >= '" + startDateStr + "' AND PODate <= '" + endDateStr + "'";
                }
                //else if (!string.IsNullOrEmpty(poStartDate) && string.IsNullOrEmpty(poEndDate))
                //{
                //    cmd.CommandText = "select * from PurchaseOrder where PODate >= '" + poEndDate + "'";
                //}
                else if (DateTime.TryParse(poStartDate, out startDate) && !DateTime.TryParse(poEndDate, out _))
                {
                    // Convert the start date to yyyy-MM-dd format
                    string startDateStr = startDate.ToString("yyyy-MM-dd");

                    cmd.CommandText = "SELECT * FROM PurchaseOrder WHERE PODate >= '" + startDateStr + "'";
                }
                else
                {
                    cmd.CommandText = "select * from PurchaseOrder";
                }
                var reader = cmd.ExecuteReader();

                List<PurchaseOrder> poList = new List<PurchaseOrder>();
                while (reader.Read())
                {
                    PurchaseOrder po = new PurchaseOrder();
                    po = ConvertObject(reader);
                    poList.Add(po);
                }
                conn.Close();
                return poList;
            }
        }

        public static bool SaveData(PurchaseOrder po, bool IsNew = true)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);

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

                if (IsNew)
                {
                    string insertStatement = string.Format("INSERT INTO PurchaseOrder" +
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
                    "VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}',{12},'{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}')"
                    , po.PONumber
                    , po.PODate
                    , po.Vendor
                    , po.QuoteRef
                    , po.HSNNo
                    , po.PORemarks
                    , po.DeliveryRequiredBy
                    , po.DeliveryInstructions
                    , po.DeliveryRequiredAt
                    , po.TransportationCharges
                    , po.PaymentTerms
                    , po.Company
                    , po.POAmount
                    , po.ProjectRef
                    , po.AllProjects
                    , DateTime.Now.ToString()
                    , "yes"
                    , po.BillAmount
                    , po.BillNoAndDate
                    , po.PaymentDate
                    , po.DisplayTotal
                    , po.DisplayDiscount);

                    cmd.CommandText = insertStatement;
                }
                else
                {
                    string updateStatement = string.Format("UPDATE PurchaseOrder SET " +
                    " PODate='{0}'" +
                    ",Vendor='{1}'" +
                    ",QuoteRef='{2}'" +
                    ",HSN='{3}'" +
                    ",PORemarks='{4}'" +
                    ",DeliveryRequiredBy='{5}'" +
                    ",DeliveryInstructions='{6}'" +
                    ",DeliveryRequiredAt='{7}'" +
                    ",TransportationCharges='{8}'" +
                    ",PaymentTerms='{9}'" +
                    ",Company='{10}'" +
                    ",POAmount='{11}'" +
                    ",ProjectRef='{12}'" +
                    ",AllProjects='{13}'" +
                    ",UpdatedOn='{14}'" +
                    ",BillAmount='{15}'" +
                    ",BillNoAndDate='{16}'" +
                    ",PaymentDate='{17}'" +
                    ",DisplayTotal='{19}'" +
                    ",DisplayDiscount='{20}'" +
                    " WHERE PONumber='{18}' "
                    , po.PODate
                    , po.Vendor
                    , po.QuoteRef
                    , po.HSNNo
                    , po.PORemarks
                    , po.DeliveryRequiredBy
                    , po.DeliveryInstructions
                    , po.DeliveryRequiredAt
                    , po.TransportationCharges
                    , po.PaymentTerms
                    , po.Company
                    , po.POAmount
                    , po.ProjectRef
                    , po.AllProjects
                    , DateTime.Now.ToString()
                    , po.BillAmount
                    , po.BillNoAndDate
                    , po.PaymentDate
                    , po.PONumber
                    ,po.DisplayTotal
                    ,po.DisplayDiscount
                    );
                    cmd.CommandText = updateStatement;
                }
                var res = cmd.ExecuteNonQuery();
                if (res > 0)
                {
                    //POItems delete - the entire set will be inserted again as a new entry for a PO Update
                    string deleteStatement = string.Format("DELETE FROM PurchaseOrderItem WHERE PONumber = '{0}'", po.PONumber);
                    cmd.CommandText = deleteStatement;
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                return true;
            }
        }

        public static bool RemovePO(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);

                string updateStatement = string.Format("UPDATE PurchaseOrder SET " +
                " IsActive='no'" +
                " WHERE PONumber='{0}' "
                , poNumber
                );
                cmd.CommandText = updateStatement;

                var res = cmd.ExecuteNonQuery();
                if (res > 0)
                {
                    //POItems delete - 
                    string deleteStatement = string.Format("DELETE FROM PurchaseOrderItem WHERE PONumber = '{0}'", poNumber);
                    cmd.CommandText = deleteStatement;
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                return true;
            }
        }

        public static bool ApprovePO(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);

                string updateStatement = string.Format("UPDATE PurchaseOrder SET " +
                " IsApproved='yes'" +
                " WHERE PONumber='{0}' "
                , poNumber
                );
                cmd.CommandText = updateStatement;

                var res = cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
        }

        public static bool SubmitForApproval(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);

                string updateStatement = string.Format("UPDATE PurchaseOrder SET " +
                " IsApprovalRequested='yes'" +
                " WHERE PONumber='{0}' "
                , poNumber
                );
                cmd.CommandText = updateStatement;

                var res = cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
        }
    }
}