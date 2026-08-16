using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.DAL
{

    public static class VendorDAL
    {
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        public static List<string> GetVendorNames(string vendorType = "PO")
        {

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select Name from VendorMaster where IsActive='yes' and  UPPER(VendorType) = '" + vendorType.ToUpper() + "' order by Name";
                var reader = cmd.ExecuteReader();
                List<string> vendorNames = new List<string>();
                while (reader.Read())
                {
                    vendorNames.Add(reader[0].ToString());
                }

                conn.Close();
                vendorNames = vendorNames.OrderBy(x => x).ToList();
                return vendorNames;
            }
        }


        public static List<string> GetOutgoingChallanVendorNames()
        {

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select Name from VendorMaster where IsActive='yes' and IsOutgoingVendor='yes' order by Name";
                var reader = cmd.ExecuteReader();
                List<string> vendorNames = new List<string>();
                while (reader.Read())
                {
                    vendorNames.Add(reader[0].ToString());
                }

                conn.Close();
                vendorNames = vendorNames.OrderBy(x => x).ToList();
                return vendorNames;
            }
        }

		public static string GetVendorGST(string vendorName)
		{
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select GST from VendorMaster where Name = '" + vendorName + "'";
                var reader = cmd.ExecuteReader();
                List<string> vendorGST = new List<string>();
                while (reader.Read())
                {
                    vendorGST.Add(reader[0].ToString());
                }

                conn.Close();
                return vendorGST.FirstOrDefault();
            }
        }

		public static List<string> GetAllVendorNames()
        {

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select Name from VendorMaster order by Name";
                var reader = cmd.ExecuteReader();
                List<string> vendorNames = new List<string>();
                while (reader.Read())
                {
                    vendorNames.Add(reader[0].ToString());
                }

                conn.Close();
                vendorNames = vendorNames.OrderBy(x => x).ToList();
                return vendorNames;
            }
        }

        public static List<Vendor> GetVendors(string vendorType = "PO")
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from VendorMaster where IsActive='yes' and  UPPER(VendorType) = '" + vendorType.ToUpper()  +  "' order by Name";

                var reader = cmd.ExecuteReader();

                var vendors = new List<Vendor>();
                while (reader.Read())
                {
                    vendors.Add(ConvertObject(reader));
                }
                conn.Close();
                return vendors;
            }
        }

        public static List<Vendor> GetAllVendors()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from VendorMaster order by Name";
                var reader = cmd.ExecuteReader();

                var vendors = new List<Vendor>();
                while (reader.Read())
                {
                    vendors.Add(ConvertObject(reader));
                }
                conn.Close();
                return vendors;
            }
        }

        public static Vendor GetVendor(string vendorName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from VendorMaster where upper(trim(name)) = '" + vendorName.Trim().ToUpper() + "'";
                var reader = cmd.ExecuteReader();

                var vendors = new List<Vendor>();
                while (reader.Read())
                {
                    vendors.Add(ConvertObject(reader));
                }
                conn.Close();
                return vendors.FirstOrDefault();
            }
        }

        private static Vendor ConvertObject(SQLiteDataReader reader)
        {
            Vendor vendor = new Vendor();
            vendor.VendorName = reader["Name"].ToString();
            vendor.Address = reader["Address"].ToString();
            vendor.ContactPerson = reader["ContactPerson"].ToString();
            vendor.ContactNumber = reader["ContactNumber"].ToString();
            vendor.Email = reader["Email"].ToString();
            vendor.GST = reader["GST"]?.ToString();
            vendor.VendorMSME = reader["VendorMSME"]?.ToString();
            vendor.IsActive = reader["IsActive"]?.ToString();
            vendor.VendorType = reader["VendorType"]?.ToString();
            return vendor;
        }
        
        public static string SaveData(Vendor vendor)
        {
            var vendors = GetAllVendors();
            if (vendors != null && vendors.Count() > 0)
            {
                if (vendors.Exists(x => x.VendorName == vendor.VendorName))
                {
                    return UpdateVendor(vendor);
                }
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                string insertStatement = string.Format("INSERT INTO VendorMaster(Name, Address,ContactPerson, ContactNumber, Email, GST, IsActive,VendorType,VendorMSME) " +
                    "VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')", vendor.VendorName, vendor.Address.Replace("'", "''"), vendor.ContactPerson, vendor.ContactNumber, vendor.Email, vendor.GST, vendor.IsActive,vendor.VendorType,vendor.VendorMSME);
                cmd.CommandText = insertStatement;
                cmd.ExecuteNonQuery();
                conn.Close();
                return "Added Successfully";
            }
        }

        public static string UpdateVendor(Vendor vendor)
        {

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);

                string updateStatement = string.Format("UPDATE VendorMaster " +
                    "SET Name='{0}'" +
                    ",Address = '{1}'" +
                    ",ContactPerson = '{2}'" +
                    ",ContactNumber = '{3}'" +
                    ",Email = '{4}'" +
                    ",GST='{5}'" +
                    ",IsActive='{6}',VendorType='{7}',VendorMSME='{8}' where Name = '" + vendor.VendorName + "'"
                    , vendor.VendorName, vendor.Address.Replace("'", "''"), vendor.ContactPerson, vendor.ContactNumber
                    , vendor.Email, vendor.GST, vendor.IsActive, vendor.VendorType,vendor.VendorMSME);
                cmd.CommandText = updateStatement;
                int rowsaffected = cmd.ExecuteNonQuery();
                conn.Close();
                if (rowsaffected == 0)
                {
                    return "Record Not Found";
                }
                else
                {
                    return "Updated Successfully";
                }
            }
        }

    
    }
}