using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class Country
    {
        public int countryCode;

        [Display(Name = "Country")]
        [StringLength(50)]
        public string countryName;

        public string seperator { get; set; }
    }
}
