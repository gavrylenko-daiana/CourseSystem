using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services;

public class UserCourseService : GenericService<UserCourses>, IUserCourseService
{
    public UserCourseService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.UserCoursesRepository;
    }

    public async Task CreateUserCourses(UserCourses userCourses)
    {
        try
        {
            await Add(userCourses);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create userCourses {userCourses.Id}. Exception: {ex.Message}");
        }
    }
}