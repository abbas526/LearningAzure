using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;

namespace OrientalApplication.DAL
{  
    public sealed class Database
    {
        private static volatile SQLiteConnection instance;
        private static object syncRoot = new object();
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        private Database() { }

        public static SQLiteConnection Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SQLiteConnection(connectionString);
                    }
                }

                return instance;
            }
        }
    }
}