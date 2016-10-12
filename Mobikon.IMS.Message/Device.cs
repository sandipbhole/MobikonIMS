using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class Device
    {
        public long deviceID { get; set; }

        [Display(Name = "Device Serial")]
        [Required(ErrorMessage = "Please provide device serial.", AllowEmptyStrings = false)]
        [StringLength(25)]
        public string productSerial { get; set; }

        [Display(Name = "Device Type")]
        [Required(ErrorMessage = "Please provide device type.", AllowEmptyStrings = false)]
        [StringLength(50)]
        public string deviceType { get; set; }

        [Display(Name = "Device Details")]
        [Required(ErrorMessage = "Please provide device details.", AllowEmptyStrings = false)]
        [StringLength(250)]
        public string deviceDetails { get; set; }

        public int statusID { get; set; }

        [Display(Name = "Status")]
        public string status { get; set; }

        [Display(Name = "Remarks/Note")]
        [StringLength(5000)]
        public string note { get; set; }

        public long userID { get; set; }

        [StringLength(50)]
        [Display(Name = "User Name")]
        public string userName { get; set; }

        [Display(Name = "Company Owned")]
        public string companyOwner { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [Display(Name = "Blocked Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public System.Nullable<DateTime> blockedDate { get; set; }

        [StringLength(25)]
        [Display(Name = "Device Tag")]
        public string deviceTag { get; set; }

        public string seperator { get; set; }

        public System.Nullable<long> serialNo { get; set; }

        public System.Nullable<bool> currentStatus { get; set; }
    }
}
