using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Domain.Entities;
using VillaHub.Infrastructure.Data;

namespace VillaHub.Infrastructure.Repository
{
    public class FloorRepository : Repository<Floor>, IFloorRepository
    {
        private readonly ApplicationDbContext dbContext;
        public FloorRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public Floor? GetOne(
            Expression<Func<Floor, bool>> expression,
            Func<IQueryable<Floor>, IQueryable<Floor>>? include = null,
            bool noTracking = true)
        {
            IQueryable<Floor> query = dbContext.Floors;

            if (noTracking)
                query = query.AsNoTracking();

            if (include != null)
                query = include(query);

            return query.FirstOrDefault(expression);
        }
    }
}
