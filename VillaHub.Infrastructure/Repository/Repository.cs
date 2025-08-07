using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Infrastructure.Data;

namespace VillaHub.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext dbContext)
        {
            _context = dbContext;

            _dbSet = _context.Set<T>();
        }

        //CRUD
        public async Task<bool> CreateAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");

                return false;
            }
        }


        public bool Update(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return false;
            }
        }


        public bool Delete(T entity)
        {
            try
            {
                _dbSet.Remove(entity);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return false;
            }
        }


        public T? GetOne(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool NoTracking = true
            )
        {
            return Get(expression, includes, NoTracking).FirstOrDefault();

        }



        public IEnumerable<T> Get(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool NoTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
            )
        {
            IQueryable<T> entities = _dbSet;

            if (expression != null)
            {
                entities = entities.Where(expression);
            }

            if (includes != null)
            {
                foreach (var item in includes)
                {
                    entities = entities.Include(item);
                }
            }

            if (NoTracking)
            {
                entities = entities.AsNoTracking();
            }

            if (orderBy != null)
            {
                entities = orderBy(entities); // Apply sorting
            }

            return entities.ToList();
        }


        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
