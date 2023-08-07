using Core.Models;

namespace BLL.Interfaces;

public interface ICourseService
{
    Task CreateCourse(Course course, AppUser currentUser);
    Task DeleteCourse(int courseId);
    Task UpdateName(int courseId, string newName);
}