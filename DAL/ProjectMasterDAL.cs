using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.DAL
{
    public class ProjectMasterDAL
    {
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        public static List<string> GetProjectNames()
        {

            SQLiteConnection conn = new SQLiteConnection(connectionString);

            conn.Open();

            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.CommandText = "select Name from ProjectMaster";
            var reader = cmd.ExecuteReader();
            List<string> projectNames = new List<string>();
            while (reader.Read())
            {
                projectNames.Add(reader[0].ToString());
            }

            conn.Close();
            projectNames = projectNames.OrderBy(x => x).ToList();
            return projectNames;
        }

        public static ProjectMaster GetProject(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                cmd.CommandText = "select * from ProjectMaster where upper(trim(name))='" + projectName.Replace("'", "''").Trim().ToUpper() + "'";
                var reader = cmd.ExecuteReader();

                var projects = new List<ProjectMaster>();
                while (reader.Read())
                {
                    projects.Add(ConvertObject(reader));
                }
                conn.Close();
                return projects.FirstOrDefault();
            }
        }

        private static ProjectMaster ConvertObject(SQLiteDataReader reader)
        {
            ProjectMaster project = new ProjectMaster();
            project.ProjectName = reader["Name"].ToString();
            project.IsActive = reader["IsActive"].ToString();
            return project;
        }

        public static string SaveData(ProjectMaster project)
        {
            var projects = GetProjectNames();
            if (projects != null && projects.Count() > 0)
            {
                if ((!string.IsNullOrEmpty(project.NewProjectName)) && projects.Exists(x => x.Trim().ToLower() == project.NewProjectName.Trim().ToLower()))
                {
                    throw new Exception("New Project Name exists in database.");                    
                }
                else if(!string.IsNullOrEmpty(project.NewProjectName))
                {
                    project.ProjectName = project.NewProjectName;
                    project.IsActive = "yes";
                }
                else if (projects.Exists(x => x == project.ProjectName))
                {
                    return UpdateProjectMaster(project);
                }
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {

                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand(conn);
                string insertStatement = string.Format("INSERT INTO ProjectMaster(Name, IsActive) " +
                    "VALUES('{0}','{1}')", project.ProjectName, project.IsActive);
                cmd.CommandText = insertStatement;
                cmd.ExecuteNonQuery();
                conn.Close();
                return "Added Successfully";
            }
        }

        public static string UpdateProjectMaster(ProjectMaster project)
        {



            SQLiteConnection conn = new SQLiteConnection(connectionString);

            conn.Open();

            SQLiteCommand cmd = new SQLiteCommand(conn);

            string updateStatement = string.Format("UPDATE ProjectMaster " +
                "SET Name='{0}'" +
                ",IsActive = '{1}'" +
                " where Name = '" + project.ProjectName + "'"
                , project.ProjectName, project.IsActive);
            cmd.CommandText = updateStatement;
            int rowsaffected = cmd.ExecuteNonQuery();
            conn.Close();
            if (rowsaffected == 0)
            {
                return "Record Not Found";
            }
            else
            {
                return "Updated Successfully";
            }

        }

        public static string CloneProject(ProjectMaster project)
        {

            if (GetProject(project.CloneProjectName) != null)
            {
                throw new Exception("Project with same name already exists, cannot create copy");
            }

            if (project != null)
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    SQLiteCommand cmd = new SQLiteCommand(conn);
                    string insertStatement = string.Format("INSERT INTO ProjectMaster(Name, IsActive) " +
                        "VALUES('{0}','{1}')", project.CloneProjectName.Replace("'", "''"), "yes");
                    cmd.CommandText = insertStatement;
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    var purchaseRequisitionList = PurchaseRequisitionDAL.GetPRs(project.ProjectName);
                    foreach (var pr in purchaseRequisitionList)
                    {
                        pr.ProjectRefDropdown = project.CloneProjectName;
                        pr.PRDate = DateTime.Now.Date.ToString("dd-MM-yyyy");
                        pr.DateRequired = DateTime.Now.Date.ToString("dd-MM-yyyy");
                        pr.PRNo = string.Empty;
                        PurchaseRequisitionDAL.SavePR(pr);
                    }
                    return "Added Successfully with PRs";
                }
            }
            else
            {
                throw new Exception("No Project found");
            }
            

        }
    }
}