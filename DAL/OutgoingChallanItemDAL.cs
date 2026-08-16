using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.DAL
{
    public class OutgoingChallanItemDAL
    {
        private static readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public static bool SaveData(OutgoingChallanItem oi)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(conn);
                    string insertStatement = string.Format("INSERT INTO OutgoingChallanItem(ChallanNumber, ItemDescription,HSN, Quantity, Rate,Amount,ProjectName) " +
                        "VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", oi.ChallanNumber, oi.ItemDescription, oi.HSN, oi.Quantity, oi.Rate,oi.Amount,oi.ProjectName);
                    cmd.CommandText = insertStatement;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static List<OutgoingChallanItem> GetChallanItems(string challanNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    SQLiteCommand cmd = new SQLiteCommand(conn);
                    cmd.CommandText = string.Format("select i.ChallanNumber,i.ItemDescription," +
                                                    "i.HSN,i.Quantity,i.Rate,i.Amount,i.ProjectName " +
                                                    "from OutgoingChallan oc " +
                                                    "join OutgoingChallanItem i on oc.ChallanNumber = i.ChallanNumber " +                                                    
                                                    "where oc.ChallanNumber='{0}'", challanNumber);
                    var reader = cmd.ExecuteReader();

                    List<OutgoingChallanItem> itemList = new List<OutgoingChallanItem>();
                    while (reader.Read())
                    {
                        itemList.Add(ConvertObjectToItem(reader));
                    }
                    conn.Close();
                    return itemList;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static OutgoingChallanItem ConvertObjectToItem(SQLiteDataReader reader)
        {
            OutgoingChallanItem item = new OutgoingChallanItem();
            item.ChallanNumber = reader["ChallanNumber"].ToString();
            item.HSN = reader["HSN"].ToString();
            item.Rate = reader["Rate"].ToString();
            item.ItemDescription = reader["ItemDescription"].ToString();
            item.Quantity = reader["Quantity"].ToString();
            item.Amount = reader["Amount"].ToString();
            item.ProjectName = reader["ProjectName"].ToString();
            return item;
        }

    }
}

