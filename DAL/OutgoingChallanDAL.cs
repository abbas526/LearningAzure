using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.DAL
{
    public class OutgoingChallanDAL
    {
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        public static OutgoingChallan GetOutgoingChallan(string ChallanNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from OutgoingChallan where ChallanNumber='" + ChallanNumber + "'";

                var reader = cmd.ExecuteReader();
                OutgoingChallan outgoingChallan = new OutgoingChallan();

                while (reader.Read())
                {
                    outgoingChallan = ConvertObject(reader);
                }
                conn.Close();
                if (outgoingChallan != null && !string.IsNullOrEmpty(outgoingChallan.ChallanNumber))
                {                    

                    var items = OutgoingChallanItemDAL.GetChallanItems(ChallanNumber);

                    outgoingChallan.OutgoingChallanItems = items;
                    outgoingChallan.Result = "Success";
                    return outgoingChallan;
                }
                return null;
            }
        }

        public static string GetLastChallanNo(string Company)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select ChallanNumber from OutgoingChallan  order by rowid desc";
                var reader = cmd.ExecuteReader();

                List<string> challanNumbers = new List<string>();
                while (reader.Read())
                {
                    
                    challanNumbers.Add(reader["ChallanNumber"].ToString());
                }
                conn.Close();
                List<string> f = challanNumbers.Where(x => x.Contains("F-"))?.OrderBy(x=>x).ToList();
                List<string> m = challanNumbers.Where(x => x.Contains("M-"))?.OrderBy(x=>x).ToList();
                if(Company == "M" )
                {
                    if (m != null && m.Count() > 0)
                        return m.LastOrDefault();
                    else
                        return "M-1039";
                }
                else if(Company == "F")
                {

                    if (f != null && f.Count() > 0)
                        return f.LastOrDefault();
                    else
                        return "F-1039";
                }
                return challanNumbers.LastOrDefault();
            }
        }

        private static OutgoingChallan ConvertObject(SQLiteDataReader reader)
        {

            OutgoingChallan oc = new OutgoingChallan();
            oc.ChallanNumber = reader["CHallanNumber"]?.ToString();
            oc.ChallanDate = Convert.ToDateTime(reader["ChallanDate"]).ToString("dd-MM-yyyy");
            oc.Company = reader["Company"]?.ToString();
            oc.Vendor = reader["Vendor"]?.ToString();            
            oc.VehicleNumber = reader["VehicleNumber"]?.ToString();
            oc.EWayBillNo = reader["EWayBillNo"]?.ToString();
            oc.Comment = reader["Comment"]?.ToString();
            try
            {
                oc.ReceivedDate = Convert.ToDateTime(reader["ReceivedDate"]).ToString("dd-MM-yyyy");
            }
            catch (Exception e)
            {

            }
            
            oc.ReceivedComment = reader["ReceivedComment"]?.ToString();
            oc.ValueOfMaterial = reader["ValueOfMaterial"]?.ToString();
            return oc;
        }

        public static bool SaveChallan(OutgoingChallan oc, bool IsNew = true)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);

                if (oc.ChallanDate.Contains("-"))
                {
                    var PODateArray = oc.ChallanDate.Split('-');
                    oc.ChallanDate = PODateArray[2] + "-" + PODateArray[1] + "-" + PODateArray[0];
                }
                if (!string.IsNullOrEmpty(oc.ReceivedDate) && oc.ReceivedDate.Contains("-"))
                {
                    var ReceivedDateArray = oc.ReceivedDate.Split('-');
                    oc.ReceivedDate = ReceivedDateArray[2] + "-" + ReceivedDateArray[1] + "-" + ReceivedDateArray[0];
                }
                if (IsNew)
                {
                    string insertStatement = string.Format("INSERT INTO OutgoingChallan" +
                    "(ChallanNumber" +
                    ",ChallanDate" +
                    ",VehicleNumber" +
                    ",EWayBillNo" +
                    ",Vendor" +                    
                    ",Company" +
                    ",Comment" +
                    ",ReceivedDate" +
                    ",ReceivedComment" +
                    ",ValueOfMaterial" +
                    ") " +
                    "VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')"
                    , oc.ChallanNumber
                    , oc.ChallanDate
                    , oc.VehicleNumber
                    , oc.EWayBillNo
                    , oc.Vendor
                    , oc.Company
                    , oc.Comment
                    ,oc.ReceivedDate
                    ,oc.ReceivedComment
                    ,oc.ValueOfMaterial
                    );

                    cmd.CommandText = insertStatement;
                }
                else
                {
                    string updateStatement = string.Format("UPDATE OutgoingChallan SET " +
                    " ChallanDate='{0}'" +
                    ",VehicleNumber='{1}'" +
                    ",EWayBillNo='{2}'" +
                    ",Vendor='{3}'" +                    
                    ",Company='{4}'" +
                    ",Comment='{5}'" +
                    ",ReceivedDate='{6}'" +
                    ",ReceivedComment='{7}'" +
                     ",ValueOfMaterial='{8}'" +
                    " WHERE ChallanNumber='{9}' "
                    , oc.ChallanDate
                    , oc.VehicleNumber
                    , oc.EWayBillNo
                    , oc.Vendor
                    , oc.Company
                    ,oc.Comment
                    ,oc.ReceivedDate
                    ,oc.ReceivedComment
                    ,oc.ValueOfMaterial
                    , oc.ChallanNumber
                    );
                    cmd.CommandText = updateStatement;
                }
                var res = cmd.ExecuteNonQuery();
                if (res > 0)
                {
                    
                    string deleteStatement = string.Format("DELETE FROM OutgoingChallanItem WHERE ChallanNumber = '{0}'", oc.ChallanNumber);
                    cmd.CommandText = deleteStatement;
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                return true;
            }
        }

        public static List<OutgoingChallanWithItem> GetAllChallans(string ProjectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select c.ChallanNumber,ChallanDate,VehicleNumber,EWayBillNo,Vendor,Company,ItemDescription,HSN,Quantity,Rate,Amount,ProjectName,Comment,ReceivedDate,ReceivedComment,ValueOfMaterial from OutgoingChallan c join OutgoingChallanItem i on c.challannumber = i.challannumber";

                if(!string.IsNullOrEmpty(ProjectName))
                {
                    cmd.CommandText += " where projectname = '" + ProjectName +  "'"; 
                }

                var reader = cmd.ExecuteReader();
                var outgoingChallanWithItemList = new List<OutgoingChallanWithItem>();

                while (reader.Read())
                {
                    outgoingChallanWithItemList.Add(ConvertObjectAll(reader));
                }
                conn.Close();
                return outgoingChallanWithItemList;
            }
        }

        private static OutgoingChallanWithItem ConvertObjectAll(SQLiteDataReader reader)
        {

            OutgoingChallanWithItem oc = new OutgoingChallanWithItem();
            oc.ChallanNumber = reader["CHallanNumber"]?.ToString();
            oc.ChallanDate = Convert.ToDateTime(reader["ChallanDate"]).ToString("dd-MM-yyyy");
            oc.Company = reader["Company"]?.ToString();
            oc.Vendor = reader["Vendor"]?.ToString();
            oc.VehicleNumber = reader["VehicleNumber"]?.ToString();
            oc.EWayBillNo = reader["EWayBillNo"]?.ToString();
            oc.Amount = reader["Amount"]?.ToString();
            oc.HSN = reader["HSN"]?.ToString();
            oc.ItemDescription = reader["ItemDescription"]?.ToString();
            oc.ProjectName = reader["ProjectName"]?.ToString();
            oc.Quantity = reader["Quantity"]?.ToString();
            oc.Rate = reader["Rate"]?.ToString();
            oc.Comment = reader["Comment"]?.ToString();
            try
            {
                oc.ReceivedDate = Convert.ToDateTime(reader["ReceivedDate"]).ToString("dd-MM-yyyy");
            }
            catch(Exception e)
            {

            }
            oc.ReceivedComment = reader["ReceivedComment"].ToString();
            oc.ValueOfMaterial = reader["ValueOfMaterial"].ToString();
            return oc;
        }

    }
}