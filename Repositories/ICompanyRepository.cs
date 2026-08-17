using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface ICompanyRepository
    {
        List<POCompany> GetCompanies();
    }
}
