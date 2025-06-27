using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Domain.Entities
{
    public class OTP
    {
        public int Id { get; set; }
        public int OTP_Number { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public DateTime RequestDateTime { get; set; }
        public DateTime ExpairationDateTime { get; set; }
        public ApplicationUser applicationUser { get; set; } = null!;
        public bool UsedByUser { get; set; }

    }
}
