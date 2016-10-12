using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Mobikon.IMS.Message
{
    public class Outlet
    {
        public long outletID { get; set; }

        [Display(Name = "Outlet")]
        [Required(ErrorMessage = "Please Provide Outlet Name.", AllowEmptyStrings = false)]
        [StringLength(500)]
        public string outletName { get; set; }

        [Display(Name = "Address")]
        [Required(ErrorMessage = "Please Provide Address.", AllowEmptyStrings = false)]
        [StringLength(5000)]
        public string address { get; set; }

        public int cityID { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessage = "Please Provide City Name.", AllowEmptyStrings = false)]
        [StringLength(50)]
        public string cityName { get; set; }      

        public int countryID { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessage = "Please Provide Country Name.", AllowEmptyStrings = false)]
        [StringLength(50)]
        public string countryName { get; set; }

        public long clientID { get; set; }

        [Display(Name = "Brand Name")]
        [Required(ErrorMessage = "Please Provide Brand.", AllowEmptyStrings = false)]
        [StringLength(250)]
        public string clientName { get; set; }

        [Display(Name = "Client Address")]
        [StringLength(5000)]
        public string clientAddress { get; set; }

        [Display(Name = "Client City")]
        [StringLength(50)]
        public string clientCity { get; set; }

        [Display(Name = "Client Country")]
        [StringLength(50)]
        public string clientCountry{ get; set; }

        [Display(Name = "Activated")]
        public bool activated { get; set; }

        [Display(Name = "Create Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        public long userID { get; set; }

        [Display(Name = "User Name")]
        [StringLength(50)]
        public string userName { get; set; }

        public string seperator { get; set; }
    }
}
