using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrientalApplication.Models
{
    public class ItemReceived
    {
        public string PRNumber { get; set; }
        public string ItemReceivedDate { get; set; }

        public string ItemReceivedOK { get; set; }

        public string ItemReceivedComment { get; set; }

        public string Result { get; set; }

        public string Vendor { get; set; }

        public string ItemReceivedChallanNo { get; set; }
    }
}