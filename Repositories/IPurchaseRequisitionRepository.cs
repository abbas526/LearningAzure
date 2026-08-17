using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IPurchaseRequisitionRepository
    {
        PurchaseRequisition GetPurchaseRequisition(string PRNo);
        PurchaseRequisition GetPurchaseRequisitionForDelete(string PRNo);
        string GetLastPRNo();
        string SavePR(PurchaseRequisition pr);
        List<string> GetAllPurchaseRequisitionNumbers();
        List<PurchaseRequisition> GetPRsForProject(string project);
        List<PurchaseRequisition> GetPendingPRs();
        List<PurchaseRequisition> GetAllPRsForProject(string projectName);
        List<PurchaseRequisition> GetPRsForPO(string poNumber);
        List<PurchaseRequisition> GetPRs(string projectName);
        List<PurchaseRequisition> GetPRsWithPOStatus(string projectName);
        List<PurchaseRequisition> GetPRs();
        List<PurchaseRequisition> GetPRs(string PRStartDate, string PREndDate);
        bool RemovePR(string prNumber);
        void DeleteItemReceived(string prNo);
        void UpdateAllItemReceivedFlag(string IsAllItemReceived, string prNo);
        string InsertItemReceived(ItemReceived itemReceived);
        List<string> GetAllPOsForPR(string prNo);
    }
}
