using Dapper;
using OrientalApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace OrientalApplication.Repositories
{
    public class ProjectMasterRepository : IProjectMasterRepository
    {
        private readonly string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";
        private readonly IPurchaseRequisitionRepository _purchaseRequisitionRepository;

        public ProjectMasterRepository() : this(new PurchaseRequisitionRepository())
        {
        }

        public ProjectMasterRepository(IPurchaseRequisitionRepository purchaseRequisitionRepository)
        {
            _purchaseRequisitionRepository = purchaseRequisitionRepository;
        }

        public List<string> GetProjectNames()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                List<string> projectNames = conn.Query<string>("select Name from ProjectMaster").ToList();
                projectNames = projectNames.OrderBy(x => x).ToList();
                return projectNames;
            }
        }

        public ProjectMaster GetProject(string projectName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                return conn.QueryFirstOrDefault<ProjectMaster>(
                    "select Name as ProjectName, IsActive from ProjectMaster where upper(trim(name)) = @ProjectName",
                    new { ProjectName = projectName.Trim().ToUpper() });
            }
        }

        public string SaveData(ProjectMaster project)
        {
            var projects = GetProjectNames();
            if (projects != null && projects.Count() > 0)
            {
                if ((!string.IsNullOrEmpty(project.NewProjectName)) && projects.Exists(x => x.Trim().ToLower() == project.NewProjectName.Trim().ToLower()))
                {
                    throw new Exception("New Project Name exists in database.");
                }
                else if (!string.IsNullOrEmpty(project.NewProjectName))
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

                conn.Execute("INSERT INTO ProjectMaster(Name, IsActive) VALUES(@ProjectName, @IsActive)", project);
                return "Added Successfully";
            }
        }

        public string UpdateProjectMaster(ProjectMaster project)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                int rowsaffected = conn.Execute(
                    "UPDATE ProjectMaster SET Name=@ProjectName, IsActive = @IsActive where Name = @ProjectName",
                    project);

                if (rowsaffected == 0)
                {
                    return "Record Not Found";
                }
                else
                {
                    return "Updated Successfully";
                }
            }
        }

        public string CloneProject(ProjectMaster project)
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

                    conn.Execute(
                        "INSERT INTO ProjectMaster(Name, IsActive) VALUES(@CloneProjectName, 'yes')",
                        project);

                    var purchaseRequisitionList = _purchaseRequisitionRepository.GetPRs(project.ProjectName);
                    foreach (var pr in purchaseRequisitionList)
                    {
                        pr.ProjectRefDropdown = project.CloneProjectName;
                        pr.PRDate = DateTime.Now.Date.ToString("dd-MM-yyyy");
                        pr.DateRequired = DateTime.Now.Date.ToString("dd-MM-yyyy");
                        pr.PRNo = string.Empty;
                        _purchaseRequisitionRepository.SavePR(pr);
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
