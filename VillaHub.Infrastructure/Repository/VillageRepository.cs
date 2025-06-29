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
    public class VillageRepository : Repository<Village>, IVillageRepository
    {
        private readonly ApplicationDbContext dbContext;
        public VillageRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
