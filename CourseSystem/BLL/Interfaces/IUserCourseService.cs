using Core.Models;

namespace BLL.Interfaces;

public interface IUserCourseService
{
    Task CreateUserCourses(UserCourses userCourses);
    Task AddTeacherToCourse(Course course, AppUser teacher);
    Task AddStudentToGroupAndCourse(UserGroups userGroups);
}