using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrientalApplication.Models
{
    public class UserModel
    {
        //public int ID { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }

        //public string Role { get; set; }
    }

    public class UserRoleModel
    {
        public string UserName { get; set; }
        public string RoleName { get; set; }
    }
}