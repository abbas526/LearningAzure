using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class QuickPO
    {
        public string PRNumberHidden { get; set; }
        public string Rate { get; set; }
        
        public string VendorForPO { get; set; }
        public string POQty { get; set; }

        public string PORemarks { get; set; }

    }
}