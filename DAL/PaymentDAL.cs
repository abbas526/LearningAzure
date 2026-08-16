using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Web;

namespace OrientalApplication.DAL
{
	public class PaymentDAL
	{
		private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;Pooling=True;Max Pool Size=100;";

		public static bool SaveVendorPaymentsWithBill(PaymentViewModel paymentViewModel, bool IsNew=true)
		{
			int paymentId = GetLastPaymentId()+1;
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{

				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);

				if (paymentViewModel.PaymentDate != null && paymentViewModel.PaymentDate.Contains("/"))
				{
					if (!string.IsNullOrEmpty(paymentViewModel.PaymentDate))
					{
						var BillDateArray = paymentViewModel.PaymentDate.Split('/');
						paymentViewModel.PaymentDate = BillDateArray[2] + "-" + BillDateArray[1] + "-" + BillDateArray[0];
					}
				}
				if (paymentViewModel.PaymentDate != null && paymentViewModel.PaymentDate.Contains("-"))
				{
					if (!string.IsNullOrEmpty(paymentViewModel.PaymentDate))
					{
						var BillDateArray = paymentViewModel.PaymentDate.Split('-');
						paymentViewModel.PaymentDate = BillDateArray[2] + "-" + BillDateArray[1] + "-" + BillDateArray[0];
					}
				}

				if (IsNew)
				{
					foreach (var bill in paymentViewModel.BillDetails)
					{
						string amt = bill.Amount;
						if(!bill.FullPaymentDone)
						{
							amt = paymentViewModel.PaymentAmount;
						}

						string insertStatement = string.Format("INSERT INTO VendorPayments" +
							"(PaymentAmount" +
							",PaymentDate" +
							",ChequeNo" +
							",OnlinePaymentRefNo" +
							",Vendor" +
							",PaymentId" +
							",BillNo" +
							") " +
							"VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')"
							, amt
							, paymentViewModel.PaymentDate
							, paymentViewModel.ChequeNumber
							, paymentViewModel.OnlineRefNo
							, paymentViewModel.Vendor
							, paymentId
							, bill.BillNo
							);

						cmd.CommandText = insertStatement;
						cmd.ExecuteNonQuery();
						paymentId = paymentId + 1;
					}


					foreach (var payment in paymentViewModel.BillDetails)
					{
						string updateStatement = string.Format("UPDATE VendorBill SET " +
						" FullPaymentDone='{1}'" +
						" WHERE BillNo='{2}' and Vendor='{3}' "
						, paymentId
						, payment.FullPaymentDone
						, payment.BillNo
						, paymentViewModel.Vendor
						);
						cmd.CommandText = updateStatement;
						cmd.ExecuteNonQuery();
					}
				}
				conn.Close();
				return true;
			}
		}

		public static int GetLastPaymentId()
		{
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{

				conn.Open();

				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select PaymentId from VendorPayments order by rowid desc limit 1";
				var reader = cmd.ExecuteReader();

				List<int> paymentIds = new List<int>();
				while (reader.Read())
				{
					paymentIds.Add(Convert.ToInt32(reader["PaymentId"]));
				}
				conn.Close();
				if(paymentIds.Count  == 0)
				{
					return 0;
				}
				return paymentIds.FirstOrDefault();
			}
		}

		public static bool SaveOnlyVendorBill(BillModel bills, bool IsNew = true)
		{
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{

				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);

				if (bills.BillDate != null && bills.BillDate.Contains("/"))
				{
					if (!string.IsNullOrEmpty(bills.BillDate))
					{
						var BillDateArray = bills.BillDate.Split('/');
						bills.BillDate = BillDateArray[2] + "-" + BillDateArray[1] + "-" + BillDateArray[0];
					}
				}

				if (bills.BillDate != null && bills.BillDate.Contains("-"))
				{
					if (!string.IsNullOrEmpty(bills.BillDate))
					{
						var BillDateArray = bills.BillDate.Split('-');
						bills.BillDate = BillDateArray[2] + "-" + BillDateArray[1] + "-" + BillDateArray[0];
					}
				}

				if (IsNew)
				{
					string insertStatement = string.Format("INSERT INTO VendorBill" +
					"(Vendor" +
					",BillNo" +
					",BillDate" +
					",BillAmount" +
					",Company" +
					") " +
					"VALUES('{0}','{1}','{2}','{3}','{4}')"
					, bills.Vendor
					, bills.BillNo
					, bills.BillDate
					, bills.BillAmount
					, bills.Company
					);

					cmd.CommandText = insertStatement;
				}
				else
				{
					string updateStatement = string.Format("UPDATE VendorBill SET " +
					" BillDate='{0}'" +
					",BillAmount='{1}'" +
					",Company='{4}'" +
					" WHERE BillNo='{2}' and Vendor='{3}' "
					, bills.BillDate
					, bills.BillAmount
					, bills.BillNo
					, bills.Vendor
					,bills.Company
					);
					cmd.CommandText = updateStatement;
				}
				var res = cmd.ExecuteNonQuery();
				if (res > 0 && bills.ChallanNoList != null && bills.ChallanNoList.Count > 0)
				{
					// First delete all existing records before below insert
					string deleteStatement = string.Format("DELETE FROM VendorBillChallan WHERE BillNo= '{0}' and Vendor= '{1}'"
				   , bills.BillNo
				   , bills.Vendor
				   );

					cmd.CommandText = deleteStatement;
					cmd.ExecuteNonQuery();

					foreach (var challanNo in bills.ChallanNoList)
					{
						string insertStatement = string.Format("INSERT INTO VendorBillChallan" +
						"(BillNo" +
						",ChallanNo" +
						",Vendor" +
						") " +
						"VALUES('{0}','{1}','{2}')"
						, bills.BillNo
						, challanNo
						, bills.Vendor
						);

						cmd.CommandText = insertStatement;
						cmd.ExecuteNonQuery();
					}

				}
				conn.Close();
				return true;
			}
		}
		public static List<BillModelForReport> GetBillsForReport(string vendor)
		{
			List<BillModelForReport> billModels = new List<BillModelForReport>();
			string sqlQuery = string.Empty;
			if (!string.IsNullOrEmpty(vendor))
			{
				sqlQuery = @"select b.*,vc.ChallanNo from VendorBill b join VendorBillChallan vc on b.vendor=vc.vendor and b.BillNo = vc.BillNo where  b.Vendor = '" + vendor + "'";
			}
			else
			{
				sqlQuery = @"select b.*,vc.ChallanNo from VendorBill b join VendorBillChallan vc on b.vendor=vc.vendor and b.BillNo = vc.BillNo";
			}
			using (SQLiteConnection connection = new SQLiteConnection(connectionString))
			{
				using (SQLiteCommand command = new SQLiteCommand(sqlQuery, connection))
				{
					// Add parameters to the command
					//command.Parameters.Add(new SQLiteParameter("@VendorName", DbType.String) { Value = vendor });

					// Open the connection
					connection.Open();

					// Execute the query
					using (SQLiteDataReader reader = command.ExecuteReader())
					{
						// Process the results
						while (reader.Read())
						{
							var billModel = new BillModelForReport();
							billModel.Vendor = reader["Vendor"]?.ToString();
							billModel.BillAmount = reader["BillAmount"]?.ToString();
							billModel.BillNo = reader["BillNo"]?.ToString();
							billModel.Company = reader["Company"]?.ToString();
							billModel.ChallanNo = reader["ChallanNo"]?.ToString();
							try
							{
								billModel.BillDate = Convert.ToDateTime(reader["BillDate"]).ToString("dd-MM-yyyy");
							}
							catch (Exception)
							{

							}
							billModel.FullPaymentDone = reader["FullPaymentDone"]?.ToString();
							billModels.Add(billModel);
						}
					}
				}
			}	

			return billModels;
		}


		public static List<BillModel> GetPendingBillData(string vendor)
		{
			List<BillModel> vendorPendingBills = new List<BillModel>();
			string sqlQuery = @"
							 select b.* from VendorBill b where (b.FullPaymentDone is null or  b.FullPaymentDone='false' or  b.FullPaymentDone='False')
							and b.Vendor = @VendorName";
			using (SQLiteConnection connection = new SQLiteConnection(connectionString))
			{
				using (SQLiteCommand command = new SQLiteCommand(sqlQuery, connection))
				{
					// Add parameters to the command
					command.Parameters.Add(new SQLiteParameter("@VendorName", DbType.String) { Value = vendor });

					// Open the connection
					connection.Open();

					// Execute the query
					using (SQLiteDataReader reader = command.ExecuteReader())
					{
						// Process the results
						while (reader.Read())
						{
							var billModel = new BillModel();
							billModel.Vendor = reader["Vendor"]?.ToString();
							billModel.BillAmount = reader["BillAmount"]?.ToString();
							billModel.BillNo = reader["BillNo"]?.ToString();
							billModel.Company = reader["Company"]?.ToString();
							billModel.FullPaymentDone  = reader["FullPaymentDone"]?.ToString();							
							try
							{
								billModel.BillDate = Convert.ToDateTime(reader["BillDate"]).ToString("dd-MM-yyyy");
							}
							catch (Exception)
							{

							}
							vendorPendingBills.Add(billModel);
						}
					}
				}
			}

			var vp = GetVendorPaymentsAll(vendor);
			foreach (var pendingBill in vendorPendingBills)
			{
				Double AmountAlreadyPaid = 0;
				List<VendorPayments> paymentAmounts = vp.Where(x => x.BillNo == pendingBill.BillNo).ToList();
				if(paymentAmounts != null && paymentAmounts.Count >=0)
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

		public static BillModel GetOnlyBillData(string BillNo, string vendor)
		{
			BillModel billModel = null;
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{

				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select * from VendorBill where BillNo = '" + BillNo + "' and vendor = '" + vendor + "'";

				var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					billModel = new BillModel();
					billModel.Vendor = reader["Vendor"]?.ToString();
					billModel.BillAmount = reader["BillAmount"]?.ToString();
					billModel.BillNo = reader["BillNo"]?.ToString();
					billModel.Company = reader["Company"]?.ToString();
					try
					{
						billModel.BillDate = Convert.ToDateTime(reader["BillDate"]).ToString("dd-MM-yyyy");
					}
					catch (Exception)
					{

					}
					//return billModel;
				}
				conn.Close();
			}
			var challanNos = new List<string>();
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{

				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select * from VendorBillChallan where BillNo = '" + BillNo + "' and vendor = '" + vendor + "'";

				var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					challanNos.Add(reader["ChallanNo"]?.ToString());
					//return billModel;
				}
				conn.Close();
			}
			if (billModel != null)
			{
				billModel.ChallanNoList = challanNos;
			}
			return billModel;
		}

		private static BillsAndPaymentModel ConvertObject(SQLiteDataReader reader)
		{
			BillsAndPaymentModel billsAndPaymentModel = new BillsAndPaymentModel();
			billsAndPaymentModel.Vendor = reader["Vendor"]?.ToString();
			billsAndPaymentModel.BillAmount = reader["BillAmount"]?.ToString();
			billsAndPaymentModel.BillNo = reader["BillNo"]?.ToString();
			try
			{
				billsAndPaymentModel.BillDate = Convert.ToDateTime(reader["BillDate"]).ToString("dd-MM-yyyy");
			}
			catch (Exception)
			{

			}
			return billsAndPaymentModel;
		}

		public static List<string> GetPendingChallanNumbers(string vendor)
		{
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				vendor = vendor.ToUpper();
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = @"select distinct pr.ChallanNo from PRItemReceived pr 
				where pr.ChallanNo is not null and upper(pr.Vendor) = @Vendor
				and pr.ChallanNo not in (select distinct challanNo from VendorBillChallan where upper(pr.Vendor) = @Vendor )";
				cmd.Parameters.AddWithValue("@Vendor", vendor);
				var reader = cmd.ExecuteReader();



				List<string> challanList = new List<string>();
				while (reader.Read())
				{
					challanList.Add(reader["ChallanNo"].ToString());
				}
				conn.Close();
				return challanList;
			}
		}

		public static List<string> GetVendorsWithOutstanding()
		{
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);

				cmd.CommandText = "select DISTINCT pr.Vendor from PRItemReceived pr " +
				" where pr.ChallanNo is not null and pr.ChallanNo <> ''  order by pr.Vendor";
				var reader = cmd.ExecuteReader();

				List<string> vendorList = new List<string>();
				while (reader.Read())
				{
					vendorList.Add(reader["Vendor"].ToString());
				}
				conn.Close();
				return vendorList;
			}
		}

		public static List<string> GetVendorsforDashboard()
		{
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select DISTINCT pr.Vendor from VendorBill pr";
				var reader = cmd.ExecuteReader();

				List<string> vendorList = new List<string>();
				while (reader.Read())
				{
					vendorList.Add(reader["Vendor"].ToString());
				}
				conn.Close();
				return vendorList;
			}
		}

		public static List<VendorPayments> GetVendorPayments(string vendor)
		{
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select * from VendorPayments where vendor = '" + vendor + "' order by paymentId desc Limit 20";

				var reader = cmd.ExecuteReader();

				List<VendorPayments> vendorPaymentsList = new List<VendorPayments>();

				while (reader.Read())
				{
					var vendorPayments = new VendorPayments();
					vendorPayments.Vendor = reader["vendor"].ToString();
					vendorPayments.ChequeNo = reader["ChequeNo"].ToString();
					vendorPayments.OnlinePaymentRefNo = reader["OnlinePaymentRefNo"].ToString();
					vendorPayments.PaymentAmount = reader["PaymentAmount"].ToString();
					vendorPayments.PaymentId = reader["PaymentId"].ToString();
					vendorPayments.BillNo = reader["BillNo"].ToString();
					try
					{
						vendorPayments.PaymentDate = Convert.ToDateTime(reader["PaymentDate"]).ToString("dd-MM-yyyy");
					}
					catch (Exception)
					{

					}
					vendorPaymentsList.Add(vendorPayments);
				}
				//reader = null;
				//foreach (var vp in vendorPaymentsList)
				//{
				//	SQLiteCommand cmd1 = new SQLiteCommand(conn);
				//	StringBuilder sb = new StringBuilder();

				//	cmd1.CommandText = "select * from VendorBill where BillNo = '" + vp.BillNo + "'";

				//	var reader1 = cmd1.ExecuteReader();
				//	vp.bills = new List<BillModel>();

				//	while (reader1.Read())					
				//	{
				//		var b = new BillModel();

				//		b.Vendor = reader1["Vendor"]?.ToString();
				//		b.BillAmount = reader1["BillAmount"]?.ToString();
				//		b.BillNo = reader1["BillNo"]?.ToString();
				//		sb.Append(b.BillNo + ",");
				//		try
				//		{
				//			b.BillDate = Convert.ToDateTime(reader1["BillDate"]).ToString("dd-MM-yyyy");
				//		}
				//		catch (Exception)
				//		{

				//		}
				//		vp.bills.Add(b);
				//	}
				//	vp.BillList = sb.ToString();
				//	vp.BillList = vp.BillList.TrimEnd(',');
				//}
				conn.Close();
				return vendorPaymentsList;
			}
		}

		public static List<VendorPayments> GetVendorPaymentsAll(string vendor)
		{
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select * from VendorPayments where vendor = '" + vendor + "' ";

				var reader = cmd.ExecuteReader();

				List<VendorPayments> vendorPaymentsList = new List<VendorPayments>();

				while (reader.Read())
				{
					var vendorPayments = new VendorPayments();
					vendorPayments.Vendor = reader["vendor"].ToString();
					vendorPayments.ChequeNo = reader["ChequeNo"].ToString();
					vendorPayments.OnlinePaymentRefNo = reader["OnlinePaymentRefNo"].ToString();
					vendorPayments.PaymentAmount = reader["PaymentAmount"].ToString();
					vendorPayments.PaymentId = reader["PaymentId"].ToString();
					vendorPayments.BillNo = reader["BillNo"].ToString();
					try
					{
						vendorPayments.PaymentDate = Convert.ToDateTime(reader["PaymentDate"]).ToString("dd-MM-yyyy");
					}
					catch (Exception)
					{

					}
					vendorPaymentsList.Add(vendorPayments);
				}
				conn.Close();
				return vendorPaymentsList;
			}
		}

		public static List<VendorPaymentSummary> GetPaymentSummary()
		{
			Dictionary<string, double> payments = new Dictionary<string, double>();
			Dictionary<string, double> bills = new Dictionary<string, double>();
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select vendor,sum(PaymentAmount) as total from VendorPayments group by vendor";
				var reader = cmd.ExecuteReader();


				while (reader.Read())
				{
					payments.Add(reader["Vendor"].ToString(), Convert.ToDouble(reader["total"]));
				}
				conn.Close();

			}
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select vendor,sum(BillAmount) as total from VendorBill group by vendor";
				var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					bills.Add(reader["Vendor"].ToString(), Convert.ToDouble(reader["total"]));
				}
				conn.Close();
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

		public static VendorPaymentSummary GetTotalPaymentSummary()
		{
			var summaryList = new VendorPaymentSummary();
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select sum(PaymentAmount) as total from VendorPayments";
				var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					summaryList.TotalAmountPaid = Convert.ToString(reader["total"]);
				}
				conn.Close();

			}
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select sum(BillAmount) as total from VendorBill";
				var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					summaryList.TotalBillAmount = Convert.ToString(reader["total"]);
				}
				conn.Close();
			}
			summaryList.Balance = (Convert.ToDouble(summaryList.TotalBillAmount) - Convert.ToDouble(summaryList.TotalAmountPaid)).ToString();
			return summaryList;
		}

		#region extras
		public static BillsAndPaymentModel GetBillData(string BillNo, string vendor)
		{
			using (SQLiteConnection conn = new SQLiteConnection(connectionString))
			{

				conn.Open();
				SQLiteCommand cmd = new SQLiteCommand(conn);
				cmd.CommandText = "select * from VendorBill where BillNo = '" + BillNo + "' and vendor = '" + vendor + "'";

				var reader = cmd.ExecuteReader();

				BillsAndPaymentModel billsAndPaymentModel = null;
				while (reader.Read())
				{

					billsAndPaymentModel = ConvertObject(reader);
				}
				conn.Close();
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