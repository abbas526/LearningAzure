using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IPurchaseOrderItemRepository
    {
        bool SaveData(PurchaseOrderItem poi);
        List<PurchaseOrderItem> GetPurchaseOrderItems(string poNumber);
        PurchaseOrderItem GetPurchaseOrderItem(string prNumber);
        List<PurchaseOrderItem> GetPurchaseOrderItems();
        List<PurchaseOrderItem> GetPurchaseOrderItems(string poStartDate, string poEndDate);
    }
}
