using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class City
    {
        public int cityID { get; set; }

        [Display(Name = "City")]
        [StringLength(50)]
        [Required(ErrorMessage = "Please provide city.", AllowEmptyStrings = false)]
        public string cityName { get; set; }

        public int countryID { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessage = "Please provide country.", AllowEmptyStrings = false)]
        public string countryName { get; set; }

        public string seperator { get; set; }
    }
}
