using OrientalApplication.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OrientalApplication.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.User;

            if (user != null && user.Identity.IsAuthenticated)
            {
                // Retrieve user roles from the database
                var roles = UserDAL.GetUserRoles(user.Identity.Name);

                // Check if the user has at least one required role
                return roles.Exists(role => Roles.Split(',').Contains(role));
            }
            return false;
        }
    }
}