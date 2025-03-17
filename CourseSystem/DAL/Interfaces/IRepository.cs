﻿using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAsync(
            Expression<Func<T, bool>> filter = null,
            Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBy = null);
        Task AddAsync(T entity);
        Task DeleteAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteEntityByKeys(params object[] keys);
    }
}