using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IOutgoingChallanItemRepository
    {
        bool SaveData(OutgoingChallanItem oi);
        List<OutgoingChallanItem> GetChallanItems(string challanNumber);
    }
}
