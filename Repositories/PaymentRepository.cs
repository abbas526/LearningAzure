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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;Pooling=True;Max Pool Size=100;";

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

        public bool SaveVendorPaymentsWithBill(PaymentViewModel paymentViewModel, bool IsNew = true)
        {
            int paymentId = GetLastPaymentId() + 1;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                paymentViewModel.PaymentDate = NormalizeDateForStorage(paymentViewModel.PaymentDate);

                if (IsNew)
                {
                    foreach (var bill in paymentViewModel.BillDetails)
                    {
                        string amt = bill.Amount;
                        if (!bill.FullPaymentDone)
                        {
                            amt = paymentViewModel.PaymentAmount;
                        }

                        conn.Execute(
                            "INSERT INTO VendorPayments" +
                            "(PaymentAmount" +
                            ",PaymentDate" +
                            ",ChequeNo" +
                            ",OnlinePaymentRefNo" +
                            ",Vendor" +
                            ",PaymentId" +
                            ",BillNo" +
                            ") " +
                            "VALUES(@PaymentAmount,@PaymentDate,@ChequeNo,@OnlinePaymentRefNo,@Vendor,@PaymentId,@BillNo)",
                            new
                            {
                                PaymentAmount = amt,
                                paymentViewModel.PaymentDate,
                                ChequeNo = paymentViewModel.ChequeNumber,
                                OnlinePaymentRefNo = paymentViewModel.OnlineRefNo,
                                paymentViewModel.Vendor,
                                PaymentId = paymentId,
                                bill.BillNo
                            });
                        paymentId = paymentId + 1;
                    }


                    foreach (var payment in paymentViewModel.BillDetails)
                    {
                        conn.Execute(
                            "UPDATE VendorBill SET " +
                            " FullPaymentDone=@FullPaymentDone" +
                            " WHERE BillNo=@BillNo and Vendor=@Vendor ",
                            new
                            {
                                payment.FullPaymentDone,
                                payment.BillNo,
                                paymentViewModel.Vendor
                            });
                    }
                }
                return true;
            }
        }

        public int GetLastPaymentId()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                return conn.ExecuteScalar<int>("select PaymentId from VendorPayments order by rowid desc limit 1");
            }
        }

        public bool SaveOnlyVendorBill(BillModel bills, bool IsNew = true)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                bills.BillDate = NormalizeDateForStorage(bills.BillDate);

                int res;
                if (IsNew)
                {
                    res = conn.Execute(
                        "INSERT INTO VendorBill" +
                        "(Vendor" +
                        ",BillNo" +
                        ",BillDate" +
                        ",BillAmount" +
                        ",Company" +
                        ") " +
                        "VALUES(@Vendor,@BillNo,@BillDate,@BillAmount,@Company)",
                        bills);
                }
                else
                {
                    res = conn.Execute(
                        "UPDATE VendorBill SET " +
                        " BillDate=@BillDate" +
                        ",BillAmount=@BillAmount" +
                        ",Company=@Company" +
                        " WHERE BillNo=@BillNo and Vendor=@Vendor ",
                        bills);
                }

                if (res > 0 && bills.ChallanNoList != null && bills.ChallanNoList.Count > 0)
                {
                    // First delete all existing records before below insert
                    conn.Execute("DELETE FROM VendorBillChallan WHERE BillNo= @BillNo and Vendor= @Vendor", bills);

                    foreach (var challanNo in bills.ChallanNoList)
                    {
                        conn.Execute(
                            "INSERT INTO VendorBillChallan" +
                            "(BillNo" +
                            ",ChallanNo" +
                            ",Vendor" +
                            ") " +
                            "VALUES(@BillNo,@ChallanNo,@Vendor)",
                            new { bills.BillNo, ChallanNo = challanNo, bills.Vendor });
                    }

                }
                return true;
            }
        }
        // BillDate/PaymentDate are declared DATE in the schema, so System.Data.SQLite
        // auto-converts them to .NET DateTime while Dapper materializes the row -- before our own
        // try/catch parsing below ever runs. A malformed stored value then throws FormatException
        // out of conn.Query() itself. Casting to TEXT keeps them as plain strings so the ConvertX
        // helpers/SafeFormatDate do the (safe) parsing instead. See
        // PurchaseRequisitionRepository.cs for the full write-up of this quirk.
        private const string VendorBillColumns =
            "BillNo, Vendor, CAST(BillDate AS TEXT) AS BillDate, BillAmount, FullPaymentDone, Company";

        private const string VendorBillColumnsPrefixed =
            "b.BillNo, b.Vendor, CAST(b.BillDate AS TEXT) AS BillDate, b.BillAmount, b.FullPaymentDone, b.Company";

        private const string VendorPaymentsColumns =
            "PaymentId, Vendor, PaymentAmount, CAST(PaymentDate AS TEXT) AS PaymentDate, ChequeNo, OnlinePaymentRefNo, BillNo";

        public List<BillModelForReport> GetBillsForReport(string vendor)
        {
            List<BillModelForReport> billModels = new List<BillModelForReport>();
            string sqlQuery = "select " + VendorBillColumnsPrefixed + ",vc.ChallanNo from VendorBill b join VendorBillChallan vc on b.vendor=vc.vendor and b.BillNo = vc.BillNo";
            object parameters = null;
            if (!string.IsNullOrEmpty(vendor))
            {
                sqlQuery += " where  b.Vendor = @Vendor";
                parameters = new { Vendor = vendor };
            }
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var rows = connection.Query(sqlQuery, parameters);
                foreach (var row in rows)
                {
                    var billModel = new BillModelForReport();
                    billModel.Vendor = Convert.ToString(row.Vendor);
                    billModel.BillAmount = Convert.ToString(row.BillAmount);
                    billModel.BillNo = Convert.ToString(row.BillNo);
                    billModel.Company = Convert.ToString(row.Company);
                    billModel.ChallanNo = Convert.ToString(row.ChallanNo);
                    billModel.BillDate = SafeFormatDate(row.BillDate);
                    billModel.FullPaymentDone = Convert.ToString(row.FullPaymentDone);
                    billModels.Add(billModel);
                }
            }

            return billModels;
        }


        public List<BillModel> GetPendingBillData(string vendor)
        {
            List<BillModel> vendorPendingBills = new List<BillModel>();
            string sqlQuery = @"
							 select " + VendorBillColumnsPrefixed + @" from VendorBill b where (b.FullPaymentDone is null or  b.FullPaymentDone='false' or  b.FullPaymentDone='False')
							and b.Vendor = @VendorName";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var rows = connection.Query(sqlQuery, new { VendorName = vendor });
                foreach (var row in rows)
                {
                    var billModel = new BillModel();
                    billModel.Vendor = Convert.ToString(row.Vendor);
                    billModel.BillAmount = Convert.ToString(row.BillAmount);
                    billModel.BillNo = Convert.ToString(row.BillNo);
                    billModel.Company = Convert.ToString(row.Company);
                    billModel.FullPaymentDone = Convert.ToString(row.FullPaymentDone);
                    billModel.BillDate = SafeFormatDate(row.BillDate);
                    vendorPendingBills.Add(billModel);
                }
            }

            var vp = GetVendorPaymentsAll(vendor);
            foreach (var pendingBill in vendorPendingBills)
            {
                Double AmountAlreadyPaid = 0;
                List<VendorPayments> paymentAmounts = vp.Where(x => x.BillNo == pendingBill.BillNo).ToList();
                if (paymentAmounts != null && paymentAmounts.Count >= 0)
                {
                    AmountAlreadyPaid = paymentAmounts.Sum(x => Convert.ToDouble(x.PaymentAmount));
                }
                if (AmountAlreadyPaid > 0)
                {
                    pendingBill.BillAmount = (Convert.ToDouble(pendingBill.BillAmount) - AmountAlreadyPaid).ToString();
                }

            }
            return vendorPendingBills;
        }

        public BillModel GetOnlyBillData(string BillNo, string vendor)
        {
            BillModel billModel = null;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                var rows = conn.Query(
                    "select " + VendorBillColumns + " from VendorBill where BillNo = @BillNo and vendor = @Vendor",
                    new { BillNo, Vendor = vendor });

                foreach (var row in rows)
                {
                    billModel = new BillModel();
                    billModel.Vendor = Convert.ToString(row.Vendor);
                    billModel.BillAmount = Convert.ToString(row.BillAmount);
                    billModel.BillNo = Convert.ToString(row.BillNo);
                    billModel.Company = Convert.ToString(row.Company);
                    billModel.BillDate = SafeFormatDate(row.BillDate);
                    //return billModel;
                }
            }
            var challanNos = new List<string>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                challanNos = conn.Query<string>(
                    "select ChallanNo from VendorBillChallan where BillNo = @BillNo and vendor = @Vendor",
                    new { BillNo, Vendor = vendor }).ToList();
            }
            if (billModel != null)
            {
                billModel.ChallanNoList = challanNos;
            }
            return billModel;
        }

        private static BillsAndPaymentModel ConvertObject(dynamic row)
        {
            BillsAndPaymentModel billsAndPaymentModel = new BillsAndPaymentModel();
            billsAndPaymentModel.Vendor = Convert.ToString(row.Vendor);
            billsAndPaymentModel.BillAmount = Convert.ToString(row.BillAmount);
            billsAndPaymentModel.BillNo = Convert.ToString(row.BillNo);
            billsAndPaymentModel.BillDate = SafeFormatDate(row.BillDate);
            return billsAndPaymentModel;
        }

        // Mirrors the try/catch date parsing every read method here used to do inline against a
        // SQLiteDataReader: a null/DBNull or unparseable date is left as null rather than thrown.
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

        public List<string> GetPendingChallanNumbers(string vendor)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                vendor = vendor.ToUpper();
                conn.Open();

                return conn.Query<string>(
                    @"select distinct pr.ChallanNo from PRItemReceived pr
					where pr.ChallanNo is not null and upper(pr.Vendor) = @Vendor
					and pr.ChallanNo not in (select distinct challanNo from VendorBillChallan where upper(pr.Vendor) = @Vendor )",
                    new { Vendor = vendor }).ToList();
            }
        }

        public List<string> GetVendorsWithOutstanding()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.Query<string>(
                    "select DISTINCT pr.Vendor from PRItemReceived pr " +
                    " where pr.ChallanNo is not null and pr.ChallanNo <> ''  order by pr.Vendor").ToList();
            }
        }

        public List<string> GetVendorsforDashboard()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.Query<string>("select DISTINCT pr.Vendor from VendorBill pr").ToList();
            }
        }

        public List<VendorPayments> GetVendorPayments(string vendor)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select " + VendorPaymentsColumns + " from VendorPayments where vendor = @Vendor order by paymentId desc Limit 20",
                    new { Vendor = vendor });

                List<VendorPayments> vendorPaymentsList = new List<VendorPayments>();

                foreach (var row in rows)
                {
                    vendorPaymentsList.Add(ToVendorPayments(row));
                }
                return vendorPaymentsList;
            }
        }

        public List<VendorPayments> GetVendorPaymentsAll(string vendor)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query(
                    "select " + VendorPaymentsColumns + " from VendorPayments where vendor = @Vendor ",
                    new { Vendor = vendor });

                List<VendorPayments> vendorPaymentsList = new List<VendorPayments>();

                foreach (var row in rows)
                {
                    vendorPaymentsList.Add(ToVendorPayments(row));
                }
                return vendorPaymentsList;
            }
        }

        private static VendorPayments ToVendorPayments(dynamic row)
        {
            var vendorPayments = new VendorPayments();
            vendorPayments.Vendor = Convert.ToString(row.Vendor);
            vendorPayments.ChequeNo = Convert.ToString(row.ChequeNo);
            vendorPayments.OnlinePaymentRefNo = Convert.ToString(row.OnlinePaymentRefNo);
            vendorPayments.PaymentAmount = Convert.ToString(row.PaymentAmount);
            vendorPayments.PaymentId = Convert.ToString(row.PaymentId);
            vendorPayments.BillNo = Convert.ToString(row.BillNo);
            vendorPayments.PaymentDate = SafeFormatDate(row.PaymentDate);
            return vendorPayments;
        }

        public List<VendorPaymentSummary> GetPaymentSummary()
        {
            Dictionary<string, double> payments = new Dictionary<string, double>();
            Dictionary<string, double> bills = new Dictionary<string, double>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query("select vendor,sum(PaymentAmount) as total from VendorPayments group by vendor");
                foreach (var row in rows)
                {
                    payments.Add(Convert.ToString(row.Vendor), Convert.ToDouble(row.total));
                }

            }
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var rows = conn.Query("select vendor,sum(BillAmount) as total from VendorBill group by vendor");
                foreach (var row in rows)
                {
                    bills.Add(Convert.ToString(row.Vendor), Convert.ToDouble(row.total));
                }
            }
            var summaryList = new List<VendorPaymentSummary>();

            foreach (KeyValuePair<string, double> entry in bills)
            {
                var summary = new VendorPaymentSummary();
                // do something with entry.Value or entry.Key
                summary.Vendor = entry.Key;
                summary.TotalBillAmount = entry.Value.ToString();
                summary.TotalAmountPaid = payments[entry.Key].ToString();
                summary.Balance = (Convert.ToDouble(summary.TotalBillAmount) - Convert.ToDouble(summary.TotalAmountPaid)).ToString();
                summaryList.Add(summary);
            }
            return summaryList;
        }

        public VendorPaymentSummary GetTotalPaymentSummary()
        {
            var summaryList = new VendorPaymentSummary();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                summaryList.TotalAmountPaid = conn.ExecuteScalar<string>("select sum(PaymentAmount) as total from VendorPayments");

            }
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                summaryList.TotalBillAmount = conn.ExecuteScalar<string>("select sum(BillAmount) as total from VendorBill");
            }
            summaryList.Balance = (Convert.ToDouble(summaryList.TotalBillAmount) - Convert.ToDouble(summaryList.TotalAmountPaid)).ToString();
            return summaryList;
        }

        #region extras
        public BillsAndPaymentModel GetBillData(string BillNo, string vendor)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                var rows = conn.Query(
                    "select " + VendorBillColumns + " from VendorBill where BillNo = @BillNo and vendor = @Vendor",
                    new { BillNo, Vendor = vendor });

                BillsAndPaymentModel billsAndPaymentModel = null;
                foreach (var row in rows)
                {
                    billsAndPaymentModel = ConvertObject(row);
                }
                return billsAndPaymentModel;
            }
        }
        #endregion
    }

    public class VendorPaymentSummary
    {
        public string Vendor { get; set; }
        public string TotalBillAmount { get; set; }
        public string TotalAmountPaid { get; set; }
        public string Balance { get; set; }
    }
}
