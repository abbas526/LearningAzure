using OrientalApplication.Repositories;
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
                // Attributes are instantiated by the MVC/CLR attribute pipeline from a
                // compile-time-constant constructor call, so there's no way to constructor-inject
                // IUserRepository here the way controllers do - new it up directly instead.
                IUserRepository userRepository = new UserRepository();

                // Retrieve user roles from the database
                var roles = userRepository.GetUserRoles(user.Identity.Name);

                // Check if the user has at least one required role
                return roles.Exists(role => Roles.Split(',').Contains(role));
            }
            return false;
        }
    }
}