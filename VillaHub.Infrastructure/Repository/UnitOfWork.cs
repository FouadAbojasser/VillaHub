using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Infrastructure.Data;

namespace VillaHub.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbcontext;
        
        public IOTPRepository OTP { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbcontext = dbContext;
            OTP = new OTPRepository(_dbcontext);
            ApplicationUser = new ApplicationUserRepository(_dbcontext);
        }

    }
}
