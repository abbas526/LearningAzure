using Dapper;
using OrientalApplication.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public List<string> GetItemNames()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // The final OrderBy below is a full re-sort, so sorting in SQL first was redundant.
                List<string> itemNames = conn.Query<string>("select Name from ItemMaster")
                    .Select(x => x.Trim())
                    .ToList();
                if (itemNames.Count() > 0)
                {
                    itemNames = itemNames.OrderBy(x => x).ToList();
                }
                return itemNames;
            }
        }

        public string SaveData(Item item)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                conn.Execute("INSERT INTO ItemMaster(Name) VALUES(@Name)", new { Name = item.ItemName?.Trim() });
                return "Added Successfully";
            }
        }
    }
}
