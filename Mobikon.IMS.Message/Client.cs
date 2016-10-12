using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class Client
    {
        public long clientID { get; set; }

        [Display(Name = "Brand")]
        [Required(ErrorMessage = "Please provide brand.", AllowEmptyStrings = false)]
        [StringLength(250)]
        public string clientName { get; set; }

        [Display(Name = "Address")]
        [Required(ErrorMessage = "Please provide address.", AllowEmptyStrings = false)]
        [StringLength(5000)]
        public string address { get; set; }

        public int cityID { get; set; }

        [Display(Name = "City")]
        [StringLength(50)]
        [Required(ErrorMessage = "Please provide city.", AllowEmptyStrings = false)]
        public string cityName { get; set; }

        public int countryID { get; set; }

        [Display(Name = "Country")]
        [StringLength(50)]
        [Required(ErrorMessage = "Please provide country.", AllowEmptyStrings = false)]
        public string countryName { get; set; }

        [Display(Name = "Activated")]
        public bool activated { get; set; }

        public long userID { get; set; }

        [Display(Name = "User Name")]
        [StringLength(50)]
        public string userName { get; set; }

        [Display(Name = "Paid Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        public string seperator { get; set; }
    }
}
