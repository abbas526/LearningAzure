using Dapper;
using OrientalApplication.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public List<POCompany> GetCompanies()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Column aliases map straight onto POCompany's properties, so Dapper
                // materializes the list without a manual reader loop.
                return conn.Query<POCompany>(
                    "select Name as CompanyName, Address as CompanyAddress, GSTNo, " +
                    "ContactName as ContactPerson, ContactNumber as ContactPersonNumber, Email " +
                    "from CompanyMaster").ToList();
                //cm join CompanyContact cc on cm.name = cc.companyname
            }
        }
    }
}
