using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mobikon.IMS.Message
{
    public class Status
    {
        public int statusID {get; set;}

        [Display(Name = "Status")]
        [StringLength(50)]
        [Required(ErrorMessage = "Please provide status.", AllowEmptyStrings = false)]
        public string statusName { get; set; }

        public string seperator { get; set; }
    }
}
