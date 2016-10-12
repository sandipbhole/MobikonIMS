using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class ChangePassword
    {
        public ChangePassword()
        {
        }
        public long userID { get; set; }

        [Display(Name = "User Name")]
        //[Required(ErrorMessage = "Please provide Username.", AllowEmptyStrings = false)]
        [StringLength(50)]
        public string userName        {            get;            set;        }

        [Display(Name = "Current Password")]
        [Required(ErrorMessage = "Please provide current password.", AllowEmptyStrings = false)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [StringLength(50)]
        public string currentPassword        {            get;            set;        }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Please provide password.", AllowEmptyStrings = false)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password  {  get;  set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Please provide confirm password.", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [StringLength(50)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string confirmPassword { get; set;}

        [Display(Name = "Reset Password")]
        public bool resetPassword { get; set; }

        [Display(Name = "Remember on this computer")]
        public bool rememberMe { get; set; }
    }
}