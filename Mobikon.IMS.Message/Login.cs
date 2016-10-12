using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly: SuppressIldasmAttribute]
namespace Mobikon.IMS.Message
{
    public class Login
    {
        public Login()
        {
        }
        public long userID { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "Please provide Username.", AllowEmptyStrings = false)]
        [StringLength(50)]
        public string userName        {            get;            set;        }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Please provide password.", AllowEmptyStrings = false)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [StringLength(50)]
        public string password        {            get;            set;        }

        [Display(Name = "Deleted")]
        public bool deleted        {            get;            set;        }

        public int roleID { get; set; }

        [Display(Name = "Role")]      
        [StringLength(50)]
        public string roleName        {            get;            set;        }

        [Display(Name = "Remember on this computer")]
        public bool RememberMe        {            get;            set;        }
    }
}