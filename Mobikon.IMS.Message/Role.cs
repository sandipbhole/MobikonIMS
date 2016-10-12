using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class Role
    {
        public int RoleID {get; set;}

        [Display(Name = "Role")]
        [StringLength(50)]
        [Required(ErrorMessage = "Please provide role.", AllowEmptyStrings = false)]
        public string RoleName { get; set; }

        public bool Deleted { get; set; }

        public string seperator { get; set; }
    }
}
