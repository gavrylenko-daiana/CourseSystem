using Core.Models;

namespace BLL.Interfaces;

public interface ICourseService : IGenericService<Course>
{
    Task<Result<bool>> CreateCourse(Course course, AppUser currentUser);
    Task<Result<bool>> DeleteCourse(int courseId);
    Task<Result<bool>> UpdateName(int courseId, string newName);
}