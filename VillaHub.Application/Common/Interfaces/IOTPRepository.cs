using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Domain.Entities;
using static System.Net.WebRequestMethods;

namespace VillaHub.Application.Common.Interfaces
{
    public interface IOTPRepository : IRepository<OTP>
    {
    }
}
