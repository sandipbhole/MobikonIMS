using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class DeviceTransactionRpt
    {
        public string BrandName { get; set; }
        public string OutletName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string DeviceDetails { get; set; }
        public string Status { get; set; }
        public string DeviceSerial { get; set; }
        public string DeploymentDate { get; set; }
        public string DC { get; set; }
        public string DCDate { get; set; }
        public string RDC { get; set; }
       
        public string RDCDate { get; set; }

        public string HIC { get; set; }
      
        public string HICDate { get; set; }
      
        public string Insured { get; set; }
      
        public string InsuranceClaim { get; set; }
       
        public string DamagedOldDevice { get; set; }
        public string CompanyOwned { get; set; }

        public string TransferOwnershipDate { get; set; }

        //public string Remarks { get; set; } 

        public string UserName { get; set; }
    }
}
