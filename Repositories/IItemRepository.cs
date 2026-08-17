using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IItemRepository
    {
        List<string> GetItemNames();
        string SaveData(Item item);
    }
}
