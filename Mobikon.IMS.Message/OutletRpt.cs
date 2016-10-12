using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Mobikon.IMS.Message
{
    public class OutletRpt
    {        
        public string OutletName { get; set; }
        
        public string Address { get; set; }
        
        public string City { get; set; }       
      
        public string Country { get; set; }               
        
        public string ClientName { get; set; }
      
        public string ClientAddress { get; set; }
       
        public string ClientCity { get; set; }
       
        public string ClientCountry { get; set; }

        public string Activated { get; set; }
      
        public string UserName { get; set; }      
    }
}
