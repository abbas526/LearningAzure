using Dapper;
using OrientalApplication.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    // Data access for the Vendor feature. Same Dapper/SQLite approach as the DAL/*.cs classes
    // (see CLAUDE.md's Data layer section) - the only difference from a *DAL class is that this
    // is instance-based behind IVendorRepository, so it can be constructor-injected and swapped
    // for a test double, rather than called as static methods.
    public class VendorRepository : IVendorRepository
    {
        private readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        // Maps the VendorMaster.Name column to Vendor.VendorName so Dapper's default
        // column-name-to-property mapping can materialize a Vendor without a manual reader loop.
        private const string VendorColumns = "Name as VendorName, Address, ContactPerson, ContactNumber, Email, GST, IsActive, VendorType, VendorMSME";

        public List<string> GetVendorNames(string vendorType = "PO")
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var vendorNames = conn.Query<string>(
                    "select Name from VendorMaster where IsActive='yes' and UPPER(VendorType) = @VendorType order by Name",
                    new { VendorType = vendorType.ToUpper() }).ToList();

                vendorNames = vendorNames.OrderBy(x => x).ToList();
                return vendorNames;
            }
        }

        public List<string> GetOutgoingChallanVendorNames()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var vendorNames = conn.Query<string>(
                    "select Name from VendorMaster where IsActive='yes' and IsOutgoingVendor='yes' order by Name").ToList();

                vendorNames = vendorNames.OrderBy(x => x).ToList();
                return vendorNames;
            }
        }

        public string GetVendorGST(string vendorName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.QueryFirstOrDefault<string>(
                    "select GST from VendorMaster where Name = @VendorName",
                    new { VendorName = vendorName });
            }
        }

        public List<string> GetAllVendorNames()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var vendorNames = conn.Query<string>("select Name from VendorMaster order by Name").ToList();

                vendorNames = vendorNames.OrderBy(x => x).ToList();
                return vendorNames;
            }
        }

        public List<Vendor> GetVendors(string vendorType = "PO")
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.Query<Vendor>(
                    $"select {VendorColumns} from VendorMaster where IsActive='yes' and UPPER(VendorType) = @VendorType order by Name",
                    new { VendorType = vendorType.ToUpper() }).ToList();
            }
        }

        public List<Vendor> GetAllVendors()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.Query<Vendor>($"select {VendorColumns} from VendorMaster order by Name").ToList();
            }
        }

        public Vendor GetVendor(string vendorName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.QueryFirstOrDefault<Vendor>(
                    $"select {VendorColumns} from VendorMaster where upper(trim(name)) = @VendorName",
                    new { VendorName = vendorName.Trim().ToUpper() });
            }
        }

        public string SaveData(Vendor vendor)
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

                conn.Execute(
                    "INSERT INTO VendorMaster(Name, Address, ContactPerson, ContactNumber, Email, GST, IsActive, VendorType, VendorMSME) " +
                    "VALUES(@VendorName, @Address, @ContactPerson, @ContactNumber, @Email, @GST, @IsActive, @VendorType, @VendorMSME)",
                    vendor);
                return "Added Successfully";
            }
        }

        public string UpdateVendor(Vendor vendor)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                int rowsaffected = conn.Execute(
                    "UPDATE VendorMaster " +
                    "SET Name=@VendorName" +
                    ",Address = @Address" +
                    ",ContactPerson = @ContactPerson" +
                    ",ContactNumber = @ContactNumber" +
                    ",Email = @Email" +
                    ",GST=@GST" +
                    ",IsActive=@IsActive,VendorType=@VendorType,VendorMSME=@VendorMSME where Name = @VendorName",
                    vendor);

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
