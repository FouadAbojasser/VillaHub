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

        public const string StatusPending_ar = "مُعلق";
        public const string StatusApproved_ar = "مُؤكد";
        public const string StatusCheckedIn_ar = "تم الدخول";
        public const string StatusCompleted_ar = "مُكتمل";
        public const string StatusCancelled_ar = "مُلغى";
        public const string StatusRefunded_ar = "مٌعاد دفعه";


        public static List<SelectListItem> CountryList_en => new()
            {
                new SelectListItem { Text = "Egypt", Value = "Egypt" },
                new SelectListItem { Text = "Palestine", Value = "Palestine" },
                new SelectListItem { Text = "Germany", Value = "Germany" },
                new SelectListItem { Text = "United States", Value = "United States" },
                new SelectListItem { Text = "Saudi Arabia", Value = "Saudi Arabia" }
            };

        public static List<SelectListItem> CountryList_ar => new()
            {
                new SelectListItem { Text = "مصر", Value = "مصر" },
                new SelectListItem { Text = "فلسطين", Value = "فلسطين" },
                new SelectListItem { Text = "ألمانيا", Value = "ألمانيا" },
                new SelectListItem { Text = "الولايات المتحدة الأمريكية", Value = "الولايات المتحدة الأمريكية" },
                new SelectListItem { Text = "المملكة العربية السعودية", Value = "المملكة العربية السعودية" }
            };


        public static readonly Dictionary<string, string> CountryCodes = new()
            {
                { "Egypt", "+20" },
                { "Palestine", "+970" },
                { "Germany", "+49" },
                { "United States", "+1" },
                { "Saudi Arabia", "+966" }
            };
    }
}
