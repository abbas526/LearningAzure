using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Web;

namespace OrientalApplication.DAL
{
    public static class PurchaseRequisitionDAL
    {
        private static readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        public static PurchaseRequisition GetPurchaseRequisition(string PRNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = string.Format("select * from PurchaseRequisition where prno='{0}' and IsActive='yes'", PRNo);
                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }

                SQLiteCommand cmdItemRec = new SQLiteCommand(conn);
                cmdItemRec.CommandText = string.Format("select * from PRItemReceived where prno='{0}'", PRNo);
                var readerItemRec = cmdItemRec.ExecuteReader();

                List<ItemReceived> itemReceivedList = new List<ItemReceived>();
                while (readerItemRec.Read())
                {
                    ItemReceived pr = ConvertObjectToItemReceived(readerItemRec);
                    itemReceivedList.Add(pr);
                }
                if (itemReceivedList.Count > 0)
                {
                    prList[0].ItemReceivedList = itemReceivedList;
                }
                conn.Close();
                return prList.FirstOrDefault();
            }
        }

        public static PurchaseRequisition GetPurchaseRequisitionForDelete(string PRNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = string.Format("select * from PurchaseRequisition where prno='{0}' and IsActive='yes' and PRNo not in (select PRNumber from PurchaseOrderItem) ", PRNo);
                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                return prList.FirstOrDefault();
            }
        }

        public static string GetLastPRNo()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select PRNo from PurchaseRequisition order by rowid desc limit 1";
                var reader = cmd.ExecuteReader();

                List<string> prNumbers = new List<string>();
                while (reader.Read())
                {
                    prNumbers.Add(reader["PRNo"].ToString());
                }
                conn.Close();
                return prNumbers.LastOrDefault();
            }
        }

        private static PurchaseRequisition ConvertObjectToPR(SQLiteDataReader reader)
        {
            if (reader != null)
            {

                PurchaseRequisition pr = new PurchaseRequisition();
                //pr.ItemReceivedObj = new ItemReceived();
                pr.PRDate = Convert.ToDateTime(reader["PRDate"]).ToString("dd-MM-yyyy");
                pr.PRNo = reader["PRNo"].ToString();
                pr.ProjectRefDropdown = reader["ProjectName"].ToString();
                pr.ItemDropdown = reader["ItemName"].ToString();
                pr.ItemSize = reader["Size"].ToString();
                pr.Specs = reader["Specs"].ToString();
                pr.Quantity = reader["Quantity"].ToString();
                try
                {
                    pr.DateRequired = Convert.ToDateTime(reader["DateRequired"]).ToString("dd-MM-yyyy");
                }
                catch (Exception ex)
                {

                }
                pr.Remark = reader["Remark"].ToString();
                pr.Drawing = reader["Drawing"].ToString();
                pr.Unit = reader["UnitOfMeasurement"].ToString();
                pr.UserCode = reader["UserCode"].ToString();
                bool hasMyColumn = (reader.GetSchemaTable().Select("ColumnName = 'PONumber'").Count() == 1);
                if (hasMyColumn)
                {
                    pr.AssignedToPO = Convert.ToString(reader["PONumber"]);
                }
                pr.IsActive = reader["IsActive"].ToString();
                pr.IsAllItemReceived = reader["IsAllItemReceived"].ToString();
                pr.Drawing = reader["Drawing"].ToString();
                return pr;
            }
            return null;
        }

        private static ItemReceived ConvertObjectToItemReceived(SQLiteDataReader reader)
        {
            ItemReceived itemReceived = new ItemReceived();
            try
            {
                itemReceived.ItemReceivedDate = Convert.ToDateTime(reader["ReceivedDate"]).ToString("dd-MM-yyyy");

            }
            catch (Exception ex)
            {

            }
            itemReceived.ItemReceivedOK = reader["Condition"].ToString();
            itemReceived.ItemReceivedComment = reader["Comment"].ToString();
            itemReceived.Vendor = reader["Vendor"].ToString();
            itemReceived.ItemReceivedChallanNo = reader["ChallanNo"].ToString();

            return itemReceived;
        }

        public static string SavePR(PurchaseRequisition pr)
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

                string item = string.Empty;
                if (!string.IsNullOrEmpty(pr.NewItem))
                {
                    item = pr.NewItem.Replace("'", "''").Trim();
                    SaveNewItem(item);
                }

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {

                    conn.Open();

                    SQLiteCommand cmd = new SQLiteCommand(conn);
                    string insertStatement = string.Format("INSERT INTO PurchaseRequisition(" +
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
                        "VALUES('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}')",
                        pr.PRNo,
                        pr.PRDate,
                        pr.ProjectRefDropdown?.Replace("'", "''"),
                        pr.ItemDropdown?.Replace("'", "''"),
                        pr.ItemSize?.Replace("'", "''"),
                        pr.Specs?.Replace("'", "''"),
                        pr.Quantity,
                        pr.DateRequired,
                        pr.Remark?.Replace("'", "''"),
                        pr.UserCode,
                        pr.Unit,
                        DateTime.Now.ToString(),
                        "yes",
                        pr.Drawing);
                    //System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/App_Data/LogFileDB.txt"), insertStatement);
                    cmd.CommandText = insertStatement;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return "Added Successfully";
                }
            }
        }

        private static string UpdatePR(PurchaseRequisition pr)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                string updateStatement = string.Format("UPDATE PurchaseRequisition " +
                    "SET PRDate='{0}'" +
                    ",ProjectName = '{1}'" +
                    ",ItemName = '{2}'" +
                    ",Size = '{3}'" +
                    ",Specs = '{4}'" +
                    ",Quantity='{5}'" +
                    ",DateRequired='{6}'" +
                    ",Remark='{7}'" +
                    ",UserCode='{8}'" +
                    ",UnitOfMeasurement='{9}'" +
                    ",UpdatedOn = '{11}'" +
                    ",Drawing = '{12}'" +
                    " where PRNo='{10}' "
                    , pr.PRDate, pr.ProjectRefDropdown.Replace("'", "''"), pr.ItemDropdown.Replace("'", "''"), pr.ItemSize.Replace("'", "''")
                    , pr.Specs.Replace("'", "''"), pr.Quantity, pr.DateRequired, pr.Remark?.Replace("'", "''"), pr.UserCode, pr.Unit, pr.PRNo, DateTime.Now.ToString(),pr.Drawing);
                cmd.CommandText = updateStatement;
                int rowsaffected = cmd.ExecuteNonQuery();
                conn.Close();
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

        private static void SaveNewItem(string newItem)
        {
            //If Item Already exists then dont save
            var itemNames = ItemDAL.GetItemNames();
            if (itemNames.Exists(x => x.ToLower() == newItem.ToLower()))
            {
                return;
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                string insertStatement = string.Format("INSERT INTO ItemMaster(Name) " +
                    "VALUES('{0}')", newItem);
                cmd.CommandText = insertStatement;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static List<string> GetAllPurchaseRequisitionNumbers()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select PRNo from PurchaseRequisition where IsActive='yes'";
                var reader = cmd.ExecuteReader();

                List<string> prNumbers = new List<string>();
                while (reader.Read())
                {
                    prNumbers.Add(reader["PRNo"].ToString());
                }
                conn.Close();
                return prNumbers;
            }
        }

        public static List<PurchaseRequisition> GetPRsForProject(string project)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from PurchaseRequisition where IsActive='yes' and upper(trim(projectname))='" + project.Trim().ToUpper() + "'  " +
                    " and PRNo not in (select PRNumber from PurchaseOrderItem)";
                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                //var poItems = PurchaseOrderItemDAL.GetPurchaseOrderItems();
                //var newPRList = new List<PurchaseRequisition>();
                //foreach (var pr in prList)
                //{
                //    if (poItems.Any(x=>x.PRNumber == pr.PRNo) == false)
                //    {
                //        newPRList.Add(pr);
                //    }
                //}
                return prList;
            }
        }

        public static List<PurchaseRequisition> GetPendingPRs()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from PurchaseRequisition  where IsActive='yes' and " +
                    " PRNo not in (select PRNumber from PurchaseOrderItem) order by PRDate Desc";
                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                return prList;
            }
        }
        public static List<PurchaseRequisition> GetAllPRsForProject(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select IFNULL(po.PONumber,'Not Mapped') as PONumber,pr.* " +
                                   " from PurchaseRequisition pr left join PurchaseOrderItem po " +
                                   "on pr.PRNo = po.PRNumber where upper(trim(pr.projectname))='" + projectName.Replace("'", "''").Trim().ToUpper() + "'";

                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                return prList;
            }
        }

        public static List<PurchaseRequisition> GetPRsForPO(string poNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select pr.* from PurchaseOrderItem po" +
                " join PurchaseRequisition pr on po.PRNumber = pr.PRNo" +
                " where pr.IsActive='yes' and po.PONumber = '" + poNumber + "'";

                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                //var poItems = PurchaseOrderItemDAL.GetPurchaseOrderItems();
                //var newPRList = new List<PurchaseRequisition>();
                //foreach (var pr in prList)
                //{
                //    if (poItems.Any(x=>x.PRNumber == pr.PRNo) == false)
                //    {
                //        newPRList.Add(pr);
                //    }
                //}
                return prList;
            }
        }

        public static List<PurchaseRequisition> GetPRs(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from PurchaseRequisition where IsActive='yes' and upper(trim(projectname))='" + projectName.Replace("'", "''").Trim().ToUpper() + "'";
                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                return prList;
            }
        }

        public static List<PurchaseRequisition> GetPRsWithPOStatus(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                string cmdText = "select IFNULL(po.PONumber,'Not Mapped') as PONumber,pr.* " +
                                   " from PurchaseRequisition pr left join PurchaseOrderItem po " +
                                   " on pr.PRNo = po.PRNumber where IsActive='yes' and upper(trim(projectname))='" + projectName.Replace("'", "''").Trim().ToUpper() + "'";

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = cmdText;
                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                return prList;
            }
        }

        public static List<PurchaseRequisition> GetPRs()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from PurchaseRequisition";
                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                return prList;
            }
        }

        public static List<PurchaseRequisition> GetPRs(string PRStartDate, string PREndDate)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);

                string cmdText = "select IFNULL(po.PONumber,'Not Mapped') as PONumber,pr.* " +
                                   " from PurchaseRequisition pr left join PurchaseOrderItem po " +
                                   " on pr.PRNo = po.PRNumber ";

                if (!string.IsNullOrEmpty(PRStartDate) && !string.IsNullOrEmpty(PREndDate))
                {
                    cmd.CommandText = cmdText + " where PRDate >= '" + PRStartDate + "' and PRDate <= '" + PREndDate + "'";
                }
                else if (!string.IsNullOrEmpty(PRStartDate) && string.IsNullOrEmpty(PREndDate))
                {
                    cmd.CommandText = cmdText + " where PRDate >= '" + PRStartDate + "'";
                }
                else
                {
                    cmd.CommandText = cmdText;
                }
                var reader = cmd.ExecuteReader();

                List<PurchaseRequisition> prList = new List<PurchaseRequisition>();
                while (reader.Read())
                {
                    PurchaseRequisition pr = new PurchaseRequisition();
                    pr = ConvertObjectToPR(reader);
                    prList.Add(pr);
                }
                conn.Close();
                return prList;
            }
        }

        public static bool RemovePR(string prNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);

                string updateStatement = string.Format("UPDATE PurchaseRequisition SET " +
                " IsActive='no'" +
                " WHERE PRNo='{0}' and PRNo not in (select PRNumber from PurchaseOrderItem) "
                , prNumber
                );
                cmd.CommandText = updateStatement;

                var res = cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
        }

        //public static string UpdateItemReceived(ItemReceived pr)
        //{
        //    if (pr.ItemReceivedDate.Contains("/"))
        //    {
        //        var ItemReceivedArray = pr.ItemReceivedDate.Split('/'); 
        //        pr.ItemReceivedDate = ItemReceivedArray[2] + "-" + ItemReceivedArray[1] + "-" + ItemReceivedArray[0];                
        //    }
        //    if (pr.ItemReceivedDate.Contains("-"))
        //    {
        //        var ItemReceivedArray = pr.ItemReceivedDate.Split('-'); 
        //        pr.ItemReceivedDate = ItemReceivedArray[2] + "-" + ItemReceivedArray[1] + "-" + ItemReceivedArray[0];
        //    }
        //    using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        //    {
        //        try
        //        {
        //            conn.Open();

        //            SQLiteCommand cmd = new SQLiteCommand(conn);
        //            string updateStatement = string.Format("UPDATE PurchaseRequisition " +
        //                "SET ItemReceivedDate='{0}'" +
        //                ",ItemReceivedOK = '{1}'" +
        //                ",ItemReceivedComment = '{2}'" +
        //                ",Vendor = '{3}'" +
        //                ",ItemReceivedChallanNo = '{4}'" +
        //                " where PRNo='{5}' "
        //                , pr.ItemReceivedDate, pr.ItemReceivedOK, pr.ItemReceivedComment,pr.Vendor,pr.ItemReceivedChallanNo,pr.PRNumber);
        //            cmd.CommandText = updateStatement;
        //            int rowsaffected = cmd.ExecuteNonQuery();
        //            conn.Close();
        //            if (rowsaffected == 0)
        //            {
        //                return "Record Not Found";
        //            }
        //            else
        //            {
        //                return "Updated Successfully";
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }

        //}

        public static void DeleteItemReceived(string prNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                string insertStatement = string.Format("DELETE FROM PRItemReceived WHERE PRNo = '{0}'", prNo);
                cmd.CommandText = insertStatement;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void UpdateAllItemReceivedFlag(string IsAllItemReceived, string prNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                string insertStatement = string.Format("UPDATE PurchaseRequisition SET IsAllItemReceived = '{0}' where PRNo = '{1}' ", IsAllItemReceived, prNo);
                cmd.CommandText = insertStatement;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static string InsertItemReceived(ItemReceived itemReceived)
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

                SQLiteCommand cmd = new SQLiteCommand(conn);
                string insertStatement = string.Format("INSERT INTO PRItemReceived(PRNo,ReceivedDate,Condition,Comment,ChallanNo,Vendor) " +
                    "VALUES('{0}','{1}','{2}','{3}','{4}','{5}')", itemReceived.PRNumber,itemReceived.ItemReceivedDate,itemReceived.ItemReceivedOK,itemReceived.ItemReceivedComment,itemReceived.ItemReceivedChallanNo,itemReceived.Vendor);
                cmd.CommandText = insertStatement;
                cmd.ExecuteNonQuery();
                conn.Close();
                return "Added Successfully";
            }
        }

        public static List<String> GetAllPOsForPR(string prNo)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select IFNULL(po.PONumber,'Not Mapped') as PONumber " +
                                   " from PurchaseRequisition pr left join PurchaseOrderItem po " +
                                   "on pr.PRNo = po.PRNumber where upper(trim(pr.PRNo))='" + prNo.Replace("'", "''").Trim().ToUpper() + "'";

                var reader = cmd.ExecuteReader();

                List<string> prList = new List<string>();
                while (reader.Read())
                {
                    prList.Add(reader["PONumber"].ToString());
                }
                conn.Close();
                return prList;
            }
        }
    }
}
