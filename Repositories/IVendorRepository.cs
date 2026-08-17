using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    // Pilot for the repository pattern described in CLAUDE.md: an interface + instance-based
    // implementation over the Vendor feature, in place of the static VendorDAL it replaces.
    public interface IVendorRepository
    {
        List<string> GetVendorNames(string vendorType = "PO");
        List<string> GetOutgoingChallanVendorNames();
        string GetVendorGST(string vendorName);
        List<string> GetAllVendorNames();
        List<Vendor> GetVendors(string vendorType = "PO");
        List<Vendor> GetAllVendors();
        Vendor GetVendor(string vendorName);
        string SaveData(Vendor vendor);
        string UpdateVendor(Vendor vendor);
    }
}
