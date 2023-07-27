using Core.Models;

namespace BLL.Interfaces;

public interface IUserCourseService : IGenericService<UserCourses>
{
    Task CreateUserCourses(UserCourses userCourses);
}