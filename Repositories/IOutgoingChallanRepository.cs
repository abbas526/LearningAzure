using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IOutgoingChallanRepository
    {
        OutgoingChallan GetOutgoingChallan(string ChallanNumber);
        string GetLastChallanNo(string Company);
        bool SaveChallan(OutgoingChallan oc, bool IsNew = true);
        List<OutgoingChallanWithItem> GetAllChallans(string ProjectName);
    }
}
