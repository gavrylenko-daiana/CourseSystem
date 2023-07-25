using Core.Models;

namespace BLL.Interfaces;

public interface ICourseService : IGenericService<Course>
{
    Task CreateCourse(Course course);
    Task DeleteCourse(int courseId);
    Task UpdateName(int courseId, string newName);
}