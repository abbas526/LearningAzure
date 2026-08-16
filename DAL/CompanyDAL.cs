using OrientalApplication.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Web;

namespace OrientalApplication.DAL
{
    public static class CompanyDAL
    {
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        public static List<POCompany> GetCompanies()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select Name,Address,GSTNo,ContactName,ContactNumber,Email " +
                    "from CompanyMaster";
                //cm join CompanyContact cc on cm.name = cc.companyname
                var reader = cmd.ExecuteReader();

                var companies = new List<POCompany>();
                while (reader.Read())
                {
                    companies.Add(ConvertObject(reader));
                }
                conn.Close();
                return companies;
            }
        }

        private static POCompany ConvertObject(SQLiteDataReader reader)
        {
            POCompany po = new POCompany();
            po.CompanyName = reader["Name"].ToString();
            po.CompanyAddress = reader["Address"]?.ToString();
            po.ContactPerson = reader["ContactName"]?.ToString();
            po.ContactPersonNumber = reader["ContactNumber"]?.ToString();
            po.Email = reader["Email"]?.ToString();
            po.GSTNo = reader["GSTNo"]?.ToString();
            return po;
        }
    }


}