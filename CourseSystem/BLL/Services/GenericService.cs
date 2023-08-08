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
}