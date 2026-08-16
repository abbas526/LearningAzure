using ClosedXML.Excel;
using OrientalApplication.DAL;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrientalApplication.DAL
{
    public class ItemDAL
    {
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public static List<string> GetItemNames()
        {
            SQLiteConnection conn = new SQLiteConnection(connectionString);

            conn.Open();

            SQLiteCommand cmd = new SQLiteCommand(conn)
            {
                CommandText = "select Name from ItemMaster order by Name asc"
            };
            var reader = cmd.ExecuteReader();
            List<string> itemNames = new List<string>();
            while (reader.Read())
            {
                itemNames.Add(reader[0].ToString().Trim());
            }

            conn.Close();
            if (itemNames.Count() > 0)
            {
                itemNames = itemNames.OrderBy(x => x).ToList();
            }
            return itemNames;
        }

        public static string SaveData(Item item)
        {

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                string insertStatement = string.Format("INSERT INTO ItemMaster(Name) " +
                    "VALUES('{0}')", item.ItemName?.Trim());
                cmd.CommandText = insertStatement;
                cmd.ExecuteNonQuery();
                conn.Close();
                return "Added Successfully";
            }
        }
    }
}