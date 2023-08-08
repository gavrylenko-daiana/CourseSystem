using Core.Models;

namespace BLL.Interfaces;

public interface IUserCourseService : IGenericService<UserCourses>
{
    Task<Result<bool>> CreateUserCourses(UserCourses userCourses);
    Task AddTeacherToCourse(Course course, AppUser teacher);
    Task AddStudentToGroupAndCourse(UserGroups userGroups);
}