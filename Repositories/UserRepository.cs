using Dapper;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString = "Data Source=" + System.Web.HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public Boolean ValidateUser(string UserName, string Password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                var users = conn.Query<UserModel>(
                    "select UserName, UserPassword from Users where upper(trim(username)) = @UserName",
                    new { UserName = UserName.Trim().ToUpper() }).ToList();

                Boolean IsValidUser = false;
                foreach (var user in users)
                {
                    if (UserName.ToUpper() == user.UserName.ToUpper() &&
                        Password == user.UserPassword)
                    {
                        IsValidUser = true;
                    }
                    else
                    {
                        IsValidUser = false;
                    }
                }

                return IsValidUser;
            }
        }

        public List<string> GetUserRoles(string UserName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.Query<string>(
                    "select RoleName from UserRoles where upper(trim(username)) = @UserName",
                    new { UserName = UserName.Trim().ToUpper() }).ToList();
            }
        }
    }
}
