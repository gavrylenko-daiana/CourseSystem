using Core.Models;

namespace BLL.Interfaces;

public interface IUserCourseService : IGenericService<UserCourses>
{
    Task CreateUserCourses(UserCourses userCourses);
    Task AddTeacherToCourse(Course course, AppUser teacher);
}