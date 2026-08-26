using Dapper;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    public class OutgoingChallanRepository : IOutgoingChallanRepository
    {
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        private readonly IOutgoingChallanItemRepository _outgoingChallanItemRepository;

        public OutgoingChallanRepository() : this(new OutgoingChallanItemRepository())
        {
        }

        public OutgoingChallanRepository(IOutgoingChallanItemRepository outgoingChallanItemRepository)
        {
            _outgoingChallanItemRepository = outgoingChallanItemRepository;
        }

        // ChallanDate/ReceivedDate are declared DATE in the schema, so System.Data.SQLite
        // auto-converts them to .NET DateTime while Dapper materializes the row -- before our own
        // try/catch parsing below ever runs. A malformed stored value then throws FormatException
        // out of conn.Query()/QueryFirstOrDefault() itself. Casting to TEXT keeps them as plain
        // strings so ConvertObject/SafeFormatDate do the (safe) parsing instead.
        // See PurchaseRequisitionRepository.cs for the full write-up of this quirk.
        private const string OutgoingChallanColumns =
            "ChallanNumber, CAST(ChallanDate AS TEXT) AS ChallanDate, VehicleNumber, EWayBillNo, Vendor, Company, Comment, " +
            "ValueOfMaterial, CAST(ReceivedDate AS TEXT) AS ReceivedDate, ReceivedComment";

        public OutgoingChallan GetOutgoingChallan(string ChallanNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var row = conn.QueryFirstOrDefault(
                    "select " + OutgoingChallanColumns + " from OutgoingChallan where ChallanNumber=@ChallanNumber",
                    new { ChallanNumber });

                OutgoingChallan outgoingChallan = row != null ? ConvertObject(row) : null;

                if (outgoingChallan != null && !string.IsNullOrEmpty(outgoingChallan.ChallanNumber))
                {
                    var items = _outgoingChallanItemRepository.GetChallanItems(ChallanNumber);

                    outgoingChallan.OutgoingChallanItems = items;
                    outgoingChallan.Result = "Success";
                    return outgoingChallan;
                }
                return null;
            }
        }

        public string GetLastChallanNo(string Company)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                List<string> challanNumbers = conn.Query<string>("select ChallanNumber from OutgoingChallan  order by rowid desc").ToList();

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

        private static OutgoingChallan ConvertObject(dynamic row)
        {
            OutgoingChallan oc = new OutgoingChallan();
            oc.ChallanNumber = Convert.ToString(row.ChallanNumber);
            oc.ChallanDate = SafeFormatDate(row.ChallanDate);
            oc.Company = Convert.ToString(row.Company);
            oc.Vendor = Convert.ToString(row.Vendor);
            oc.VehicleNumber = Convert.ToString(row.VehicleNumber);
            oc.EWayBillNo = Convert.ToString(row.EWayBillNo);
            oc.Comment = Convert.ToString(row.Comment);
            oc.ReceivedDate = SafeFormatDate(row.ReceivedDate);
            oc.ReceivedComment = Convert.ToString(row.ReceivedComment);
            oc.ValueOfMaterial = Convert.ToString(row.ValueOfMaterial);
            return oc;
        }

        // Mirrors the try/catch date parsing the original reader-based code did inline: a
        // null/DBNull or unparseable date is left as null rather than thrown.
        private static string SafeFormatDate(object rawDate)
        {
            if (rawDate == null || rawDate is DBNull)
            {
                return null;
            }
            try
            {
                return Convert.ToDateTime(rawDate).ToString("dd-MM-yyyy");
            }
            catch (Exception)
            {
                return null;
            }
        }

        // See PurchaseRequisitionRepository.NormalizeDateForStorage for why this replaces the
        // previous separator-sniffing split/reorder logic: DateTime.TryParseExact against the
        // known incoming formats always normalizes to ISO "yyyy-MM-dd" instead of risking a
        // format flip.
        private static readonly string[] KnownDateFormats = { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };

        private static string NormalizeDateForStorage(string rawDate)
        {
            if (string.IsNullOrWhiteSpace(rawDate))
            {
                return rawDate;
            }
            if (DateTime.TryParseExact(rawDate.Trim(), KnownDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return parsed.ToString("yyyy-MM-dd");
            }
            return rawDate;
        }

        public bool SaveChallan(OutgoingChallan oc, bool IsNew = true)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                oc.ChallanDate = NormalizeDateForStorage(oc.ChallanDate);
                oc.ReceivedDate = NormalizeDateForStorage(oc.ReceivedDate);

                int res;
                if (IsNew)
                {
                    res = conn.Execute(
                        "INSERT INTO OutgoingChallan" +
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
                        "VALUES(@ChallanNumber,@ChallanDate,@VehicleNumber,@EWayBillNo,@Vendor,@Company,@Comment,@ReceivedDate,@ReceivedComment,@ValueOfMaterial)",
                        oc);
                }
                else
                {
                    res = conn.Execute(
                        "UPDATE OutgoingChallan SET " +
                        " ChallanDate=@ChallanDate" +
                        ",VehicleNumber=@VehicleNumber" +
                        ",EWayBillNo=@EWayBillNo" +
                        ",Vendor=@Vendor" +
                        ",Company=@Company" +
                        ",Comment=@Comment" +
                        ",ReceivedDate=@ReceivedDate" +
                        ",ReceivedComment=@ReceivedComment" +
                        ",ValueOfMaterial=@ValueOfMaterial" +
                        " WHERE ChallanNumber=@ChallanNumber ",
                        oc);
                }
                if (res > 0)
                {
                    conn.Execute("DELETE FROM OutgoingChallanItem WHERE ChallanNumber = @ChallanNumber", oc);
                }
                return true;
            }
        }

        public List<OutgoingChallanWithItem> GetAllChallans(string ProjectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string sql = "select c.ChallanNumber,CAST(ChallanDate AS TEXT) AS ChallanDate,VehicleNumber,EWayBillNo,Vendor,Company,ItemDescription,HSN,Quantity,Rate,Amount,ProjectName,Comment,CAST(ReceivedDate AS TEXT) AS ReceivedDate,ReceivedComment,ValueOfMaterial from OutgoingChallan c join OutgoingChallanItem i on c.challannumber = i.challannumber";

                object parameters = null;
                if (!string.IsNullOrEmpty(ProjectName))
                {
                    sql += " where projectname = @ProjectName";
                    parameters = new { ProjectName };
                }

                var rows = conn.Query(sql, parameters);
                var outgoingChallanWithItemList = new List<OutgoingChallanWithItem>();

                foreach (var row in rows)
                {
                    outgoingChallanWithItemList.Add(ConvertObjectAll(row));
                }
                return outgoingChallanWithItemList;
            }
        }

        private static OutgoingChallanWithItem ConvertObjectAll(dynamic row)
        {
            OutgoingChallanWithItem oc = new OutgoingChallanWithItem();
            oc.ChallanNumber = Convert.ToString(row.ChallanNumber);
            oc.ChallanDate = SafeFormatDate(row.ChallanDate);
            oc.Company = Convert.ToString(row.Company);
            oc.Vendor = Convert.ToString(row.Vendor);
            oc.VehicleNumber = Convert.ToString(row.VehicleNumber);
            oc.EWayBillNo = Convert.ToString(row.EWayBillNo);
            oc.Amount = Convert.ToString(row.Amount);
            oc.HSN = Convert.ToString(row.HSN);
            oc.ItemDescription = Convert.ToString(row.ItemDescription);
            oc.ProjectName = Convert.ToString(row.ProjectName);
            oc.Quantity = Convert.ToString(row.Quantity);
            oc.Rate = Convert.ToString(row.Rate);
            oc.Comment = Convert.ToString(row.Comment);
            oc.ReceivedDate = SafeFormatDate(row.ReceivedDate);
            oc.ReceivedComment = Convert.ToString(row.ReceivedComment);
            oc.ValueOfMaterial = Convert.ToString(row.ValueOfMaterial);
            return oc;
        }

    }
}
