using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Application.Common.Interfaces;
using static System.Net.WebRequestMethods;
using VillaHub.Infrastructure.Data;
using VillaHub.Domain.Entities;

namespace VillaHub.Infrastructure.Repository
{
    public class OTPRepository : Repository<OTP>, IOTPRepository
    {
        private readonly ApplicationDbContext dbContext;
        public OTPRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
