using System.Linq.Expressions;

namespace BLL.Interfaces;

public interface IGenericService<T> where T : class
{
    Task<List<T>> GetAll();
    Task<T> GetById(int id);
    Task<List<T>> GetByPredicate(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);
    Task Add(T obj);
    Task Delete(int id);
    Task Update(T obj);
}