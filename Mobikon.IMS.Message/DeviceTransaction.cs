using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class DeviceTransaction
    {
        public long deviceID { get; set; }
        public long serialNo { get; set; }

        [Display(Name = "Device Serial")]
        [StringLength(50)]
        [Required(ErrorMessage = "Please provide device serial.", AllowEmptyStrings = false)]
        public string productSerial { get; set; }

        [Display(Name = "Device Details")]
        [StringLength(250)]       
        public string deviceDetails { get; set; }

        public string deviceTag { get; set; }

        public string deviceType { get; set; }

        public System.Nullable<long> clientID {get; set;}

        [Display(Name = "Brand")]
        [StringLength(50)]
        [Required(ErrorMessage = "Please provide brand.", AllowEmptyStrings = false)]
        public string clientName { get; set; }

        [Display(Name = "Address")]
        public string clientAddress { get; set; }

        [Display(Name = "City")]
        public string clientCityName { get; set; }
       

        [Display(Name = "Country")]
        public string clientCountryName { get; set; }
        
        public System.Nullable<long> outletID { get; set; }

        [Display(Name = "Outlet")]
        [StringLength(50)]
        [Required(ErrorMessage = "Please provide outlet.", AllowEmptyStrings = false)]

        public string outletName { get; set; }

        [Display(Name = "Address")]
        [StringLength(5000)]
        public string address { get; set; }

        public System.Nullable<int> countryID { get; set; }
       
        [Display(Name = "Country")]
        public string countryName { get; set; }

        public System.Nullable<int> cityID { get; set; }

        [Display(Name = "City")]
        public string cityName { get; set; }  

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Deployment Date")]      
        public System.Nullable<System.DateTime> deliveryDate { get; set; }

        [Display(Name = "Delivery Challan No.")]
        [StringLength(20)]
        public string dc { get; set; }

        [Display(Name = "Delivery Challan Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public System.Nullable<System.DateTime> dcDate { get; set; }

        [StringLength(5000)]
        [Display(Name = "Delivery Challan")]
        public string dcFile { get; set; }

        public string dcFileName { get; set; }

        [StringLength(20)]
        [Display(Name = "Returned Delivery Challan")]
        public string rdc { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Returned Delivery Challan Date")]
        public System.Nullable<System.DateTime> rdcDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Hardware Inward Challan")]
        public string hic { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hardware Inward Challan Date")]
        public System.Nullable<System.DateTime> hicDate { get; set; }

        [Display(Name = "Insured")]
        public bool insured { get; set; }

        [StringLength(25)]       
        [Display(Name = "Insurance Claim")]
        public string insuranceClaim { get; set; }

        [StringLength(25)]
        [Display(Name = "Damaged/Old Device")]       
        public string damagedOldDevice { get; set; }

        [StringLength(5)]
        [Display(Name = "Company Owned")]
        public string companyOwner { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Transfer Ownership Date")]
        public System.Nullable<System.DateTime> transferOwnershipDate { get; set; }

        [StringLength(5000)]
        [Display(Name = "Remarks/Note")]
        public string remarks { get; set; }

        public int statusID { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Please provide status.", AllowEmptyStrings = false)]
        [Display(Name = "Status")]
        public string status { get; set; }

        [Display(Name = "User ID")]
        public long userID { get; set; }

        [StringLength(50)]
        [Display(Name = "User Name")]
        public string userName { get; set; }

        public string seperator { get; set; }

        public DateTime fromDate { get; set; }

        public DateTime toDate { get; set; }

        public DateTime createdDate { get; set; }

        public int deviceCount {get; set;}

        public System.Nullable<bool> currentStatus { get; set; }
    }
}
