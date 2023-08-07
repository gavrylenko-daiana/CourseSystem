using Core.Models;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class Repository<T> : IRepository<T> where T: class
    {
        private readonly ApplicationContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                return await _dbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            }
            catch( Exception ex )
            {
                throw new Exception($"Exception message:{ex.Message}");
            }
        }

        public async Task AddAsync(T item)
        {
            try
            {
                await _dbSet.AddAsync(item);
            }
            catch(Exception ex)
            {
                throw new Exception($"Exception message:{ex.Message}");
            }
        }

        public async Task DeleteAsync(T item)
        {
            try
            {
                if (_context.Entry(item).State == EntityState.Detached)
                {
                    _dbSet.Attach(item);
                }
                _dbSet.Remove(item);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception message:{ex.Message}");
            }
        }

        public async Task UpdateAsync(T item)
        {
            try
            {
                _dbSet.Attach(item);
                _context.Entry(item).State = EntityState.Modified;
            }
            catch(Exception ex)
            {
                throw new Exception($"Exception message: {ex.Message}");
            }
        }

        public async Task<T> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception message:{ex.Message}");
            }
        }

        public virtual async Task<List<T>> GetAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (orderBy != null)
                {
                    return await orderBy(query).ToListAsync();
                }
                else
                {
                    return await query.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception message:{ex.Message}");
            }
        }

        public async Task DeleteEntityByKeys(params object[] keys)
        {
            var entity = _dbSet.Find(keys);

            if (entity == null)
                throw new Exception($"Entity {nameof(T)} wasn't found");

            try
            {
                _dbSet.Remove(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception message:{ex.Message}");
            }
        }
    }
}
