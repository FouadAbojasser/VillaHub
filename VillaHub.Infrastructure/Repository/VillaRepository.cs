using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Domain.Entities;
using VillaHub.Infrastructure.Data;

namespace VillaHub.Infrastructure.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext dbContext;
        public VillaRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
