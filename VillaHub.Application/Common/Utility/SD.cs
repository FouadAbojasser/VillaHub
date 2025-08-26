using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VillaHub.Application.Common.Utility
{
    public static class SD
    {
        public const string AdminEmial = "fouad.abojasser@gmail.com";

        public const string Role_SuperAdmin = "SuperAdmin";
        public const string Role_Admin = "Admin";
        public const string Role_Customer = "Customer";

        public const string Active_User = "Active";
        public const string Blocked_User = "Blocked";
        public const string Deleted_User = "Deleted";


        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";


        public static List<SelectListItem> CountryList = new List<SelectListItem>
        {
            new SelectListItem { Text = "Egypt", Value = "+2" },
            new SelectListItem { Text = "Palestine", Value = "+970" },
            new SelectListItem { Text = "United States", Value = "+1" },
            new SelectListItem { Text = "Germany", Value = "+49" },
            new SelectListItem { Text = "Saudi Arabia", Value = "+966" },
        };

        





    }
}
