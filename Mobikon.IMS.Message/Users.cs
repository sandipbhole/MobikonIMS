using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class User
    {

        public long userID { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "Please provide user name.", AllowEmptyStrings = false)]
        [StringLength(50)]
        public string userName { get; set; }

        [StringLength(250)]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please provide correct email.")]
        public string email { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Please provide confirm password.", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [StringLength(50)]
        [Compare("Password", ErrorMessage = "The password and confirmation do not match.")]
        public string confirmPassword { get; set; }

        [Required(ErrorMessage = "Please provide password.", AllowEmptyStrings = false)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [StringLength(50)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Current Password")]
        [Required(ErrorMessage = "Please provide current password.", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [StringLength(50)]       
        public string currentPassword { get; set; }

        public int roleID { get; set; }

        [Display(Name = "Role")]
        [Required(ErrorMessage = "Please provide role.", AllowEmptyStrings = false)]
        public string roleName { get; set; }

        public System.Nullable<bool> lockedOutEnabled { get; set; }

        [Display(Name = "Activated")]
        public bool activated { get; set; }

        public bool resetPassword { get; set; }

        [Display(Name = "Remember on this computer")]
        public bool rememberMe {get; set; }

        public string seperator { get; set; }
    }
}
