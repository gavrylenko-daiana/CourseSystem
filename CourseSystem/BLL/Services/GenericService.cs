using System.Linq.Expressions;
using BLL.Interfaces;
using DAL.Interfaces;
using DAL;
using DAL.Repository;
using Core.Models;

namespace BLL.Services;

public class GenericService<T> : IGenericService<T> where T : BaseEntity
{
    protected UnitOfWork _unitOfWork;
    protected IRepository<T> _repository;

    protected GenericService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<List<T>> GetAll()
    {
        try
        {
            return await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get all. Exception: {ex.Message}");
        }
    }

    public async Task<T> GetById(int id)
    {
        try
        {
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get {typeof(T).Name} by Id {id}. Exception: {ex.Message}");
        }
    }

    public async Task<List<T>> GetByPredicate(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
    {
        try
        {
            return await _repository.GetAsync(filter, orderBy);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get {typeof(T).Name} by predicate. Exception: {ex.Message}", ex);
        }
    }

    public async Task Add(T obj)
    {
        try
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }
            
            await _repository.AddAsync(obj);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add {typeof(T).Name}. Exception: {ex.Message}");
        }
    }

    public async Task Delete(int id)
    {
        try
        {
            var obj = await GetById(id);
            
            if (obj == null)
            {
                throw new NullReferenceException();
            }

            await _repository.DeleteAsync(obj);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete {typeof(T).Name} with Id {id}. Exception: {ex.Message}");
        }
    }

    public async Task Update(T obj)
    {
        try
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }

            await _repository.UpdateAsync(obj);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update {typeof(T).Name}. Exception: {ex.Message}");
        }
    }
}