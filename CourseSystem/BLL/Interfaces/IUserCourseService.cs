using Core.Models;

namespace BLL.Interfaces;

public interface IUserCourseService : IGenericService<UserCourses>
{
    Task<Result<bool>> CreateUserCourses(UserCourses userCourses);
    Task<Result<bool>> AddTeacherToCourse(Course course, AppUser teacher);
    Task<Result<bool>> AddStudentToGroupAndCourse(UserGroups userGroups);
}