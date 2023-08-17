using BLL.Interfaces;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;

namespace BLL.Services;

public class CourseBackgroundService : GenericService<CourseBackground>, ICourseBackgroundService
{
    private readonly IDropboxService _dropboxService;
    
    public CourseBackgroundService(UnitOfWork unitOfWork, IDropboxService dropboxService) 
        : base(unitOfWork, unitOfWork.CourseBackground)
    {
        _dropboxService = dropboxService;
    }

    public Task<Result<string>> GetRandomDefaultBackground()
    {
        throw new NotImplementedException();
    }
}