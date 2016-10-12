using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using MID = Mobikon.IMS.Data;
using MIM = Mobikon.IMS.Message;

using mC = Mobikon.IMS.Common;

namespace Mobikon.IMS.Common
{
    public class CommonMobikonIMS
    {
        public static int selectedPageSize = 0;
        public static string seperator = " | "; 

        public static List<SelectListItem> FillPaging()
        {
            List<SelectListItem> paging = new List<SelectListItem>();
            paging.Add(new SelectListItem
            {
                Text = "10",
                Value = "10",
                Selected = true
            });

            paging.Add(new SelectListItem
            {
                Text = "15",
                Value = "15",              
            });

            paging.Add(new SelectListItem
            {
                Text = "20",
                Value = "20"

            });

            paging.Add(new SelectListItem
            {
                Text = "25",
                Value = "25",
            });

            foreach (var selected in paging)
            {
                if (selected.Selected == true)
                    selectedPageSize = Convert.ToInt16(selected.Value);
            }

            return paging;
        }

        public enum StatusDevice
        {
            All = 1,  ShowSoldDevices=2 ,  NotSoldDevices=3
        };
    }
}