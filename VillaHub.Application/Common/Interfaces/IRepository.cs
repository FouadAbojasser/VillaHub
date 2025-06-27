using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Application.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<bool> CreateAsync(T entity);

        bool Update(T entity);

        bool Delete(T entity);

        T? GetOne(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool NoTracking = true
            );

        IEnumerable<T> Get(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool NoTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
            );

        Task CommitAsync();

    }
}
