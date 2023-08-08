using System.Linq.Expressions;
using BLL.Interfaces;
using DAL.Interfaces;
using DAL;
using DAL.Repository;
using Core.Models;

namespace BLL.Services;

public class GenericService<T> where T : class
{
    protected UnitOfWork _unitOfWork;
    protected IRepository<T> _repository;

    protected GenericService(UnitOfWork unitOfWork, IRepository<T> repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }
}