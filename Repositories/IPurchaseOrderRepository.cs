using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IPurchaseOrderRepository
    {
        PurchaseOrder GetPurchaseOrder(string PONo);
        List<PurchaseOrderDetails> GetAllPurchaseOrder(string projectName);
        List<PurchaseOrder> GetPurchaseOrder(string poStartDate, string poEndDate);
        bool SaveData(PurchaseOrder po, bool IsNew = true);
        bool RemovePO(string poNumber);
        bool ApprovePO(string poNumber);
        bool SubmitForApproval(string poNumber);
    }
}
