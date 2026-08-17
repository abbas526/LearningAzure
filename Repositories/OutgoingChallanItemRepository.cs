using Dapper;
using OrientalApplication.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    public class OutgoingChallanItemRepository : IOutgoingChallanItemRepository
    {
        private readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public bool SaveData(OutgoingChallanItem oi)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute(
                    "INSERT INTO OutgoingChallanItem(ChallanNumber, ItemDescription,HSN, Quantity, Rate,Amount,ProjectName) " +
                    "VALUES(@ChallanNumber,@ItemDescription,@HSN,@Quantity,@Rate,@Amount,@ProjectName)",
                    oi);
                return true;
            }
        }

        public List<OutgoingChallanItem> GetChallanItems(string challanNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.Query<OutgoingChallanItem>(
                    "select i.ChallanNumber,i.ItemDescription," +
                    "i.HSN,i.Quantity,i.Rate,i.Amount,i.ProjectName " +
                    "from OutgoingChallan oc " +
                    "join OutgoingChallanItem i on oc.ChallanNumber = i.ChallanNumber " +
                    "where oc.ChallanNumber=@ChallanNumber",
                    new { ChallanNumber = challanNumber }).ToList();
            }
        }
    }
}
