using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Domain.Entities;

namespace VillaHub.Application.Common.Interfaces
{
    public interface IFloorRepository : IRepository<Floor>
    {
        Floor? GetOne(
            Expression<Func<Floor, bool>> expression,
            Func<IQueryable<Floor>, IQueryable<Floor>>? include = null,
            bool noTracking = true);
    }
}
