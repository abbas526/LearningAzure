using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.DAL
{
    public class UserDAL
    {
        private static string connectionString = "Data Source=" + System.Web.HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        public static Boolean ValidateUser(string UserName, string Password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from Users where upper(trim(username)) = '" + UserName.Trim().ToUpper() + "'";
                var reader = cmd.ExecuteReader();
                Boolean IsValidUser = false;
                while (reader.Read())
                {
                    if (UserName.ToUpper() == reader["UserName"].ToString().ToUpper() &&
                        Password == reader["UserPassword"].ToString())
                    {
                        IsValidUser = true;
                    }
                    else
                    {
                        IsValidUser = false;
                    }
                }

                conn.Close();
                return IsValidUser;
            }
        }

        public static List<string> GetUserRoles(string UserName)
        {

            // Example using Entity Framework
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from UserRoles where upper(trim(username)) = '" + UserName.Trim().ToUpper() + "'";
                var reader = cmd.ExecuteReader();
                List<string> roles = new List<string>();
                while (reader.Read())
                {
                    roles.Add(reader["RoleName"].ToString());
                }

                conn.Close();
                return roles;
            }
        }
    }
}