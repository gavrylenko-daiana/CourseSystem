using Core.Models;

namespace BLL.Interfaces;

public interface ICourseService : IGenericService<Course>
{
    Task<Result<bool>> CreateCourse(Course course, AppUser currentUser);
    Task DeleteCourse(int courseId);
    Task UpdateName(int courseId, string newName);
}