using System;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IUserRepository
    {
        Boolean ValidateUser(string UserName, string Password);
        List<string> GetUserRoles(string UserName);
    }
}
