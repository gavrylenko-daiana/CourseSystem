using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace BLL.Interfaces
{
    public interface IGenericService<T> where T : class
    {
        Task<Result<T>> GetById(int id);
        Task<Result<List<T>>> GetByPredicate(Expression<Func<T, bool>> filter = null,
            Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBy = null);
    }
}
