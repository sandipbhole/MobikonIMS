using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class DeviceRpt
    {
        public string DeviceType { get; set; }

        public string DeviceSerial { get; set; }
       
        public string DeviceDetails { get; set; }        

        public string Status { get; set; } 
      
        public string CompanyOwned { get; set; } 
       
        public string BlockedDate { get; set; }

        public string Note { get; set; }

        public string DeviceTag { get; set; }

        public string UserName { get; set; }
    }
}
