using OrientalApplication.Models;
using System.Collections.Generic;

namespace OrientalApplication.Repositories
{
    public interface IProjectMasterRepository
    {
        List<string> GetProjectNames();
        ProjectMaster GetProject(string projectName);
        string SaveData(ProjectMaster project);
        string UpdateProjectMaster(ProjectMaster project);
        string CloneProject(ProjectMaster project);
    }
}
