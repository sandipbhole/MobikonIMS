using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class ForgetPassword
    {
        public ForgetPassword()
        {
        }
        public long userID { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "Please provide Username.", AllowEmptyStrings = false)]
        [StringLength(50)]
        public string userName { get; set; }
    }
}