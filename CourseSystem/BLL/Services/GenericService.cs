using System.Linq.Expressions;
using BLL.Interfaces;
using DAL.Interfaces;
using DAL;
using DAL.Repository;
using Core.Models;

namespace BLL.Services;

public class GenericService<T>: IGenericService<T> where T : class
{
    protected UnitOfWork _unitOfWork;
    protected IRepository<T> _repository;

    protected GenericService(UnitOfWork unitOfWork, IRepository<T> repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<Result<T>> GetById(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return new Result<T>(false, $"{typeof(T).Name} by Id {id} not found");
            }
            
            return new Result<T>(true, entity);
        }
        catch (Exception ex)
        {
            return new Result<T>(false, $"Failed to get {typeof(T).Name} by Id {id}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<List<T>>> GetByPredicate(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
    {
        try
        {
            var entityList = await _repository.GetAsync(filter, orderBy);

            if (entityList == null)
            {
                return new Result<List<T>>(false, $"{typeof(T).Name} by predicate not found");
            }
            
            return new Result<List<T>>(true, entityList);
        }
        catch (Exception ex)
        {
            return new Result<List<T>>(false, $"Failed to get {typeof(T).Name} by predicate. Exception: {ex.Message}");
        }
    }
}