using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class ProjectSummaryReportModel
    {
        public string ProjectName { get; set; }
        
        public string NumberOfPRsCreated { get; set; }

        public string NumberOfPRsWithPO { get; set; }

        public string TotalPOAmount { get; set; }
        public string OutgoingChallansCount { get; set; }
        public string ChallanItemsReceivedCount { get; set; }
        public List<ProjectSummaryPOModel> ProjectSummaryPOModel { get; set; }
    }
    public class ProjectSummaryPOModel
    {
        public List<string> PONumber { get; set; }
        public List<string> PODate { get; set; }
        public string Vendor { get; set; }       
        public string NumberOfItems { get; set; }
        public string Amount { get; set; }
        public string NumberOfItemsReceived { get; set; }
    }
}