using Core.Models;

namespace BLL.Interfaces;

public interface IUserCourseService : IGenericService<UserCourses>
{
    Task<Result<bool>> CreateUserCoursesForTests(UserCourses userCourses);
    Task<Result<bool>> AddUserInCourse(AppUser appUser, Course course);
    Task<Result<bool>> AddStudentToGroupAndCourse(UserGroups userGroups);
}