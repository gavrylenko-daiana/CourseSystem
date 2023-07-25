using BLL.Interfaces;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;

namespace BLL.Services;

public class CourseService : GenericService<Course>, ICourseService
{
    private readonly UnitOfWork _unitOfWork;
    
    protected CourseService(IRepository<Course> repository, UnitOfWork unitOfWork) : base(repository)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateCourse(Course course)
    {
        try
        {
            if (string.IsNullOrEmpty(course.Name))
            {
                throw new Exception("Course title cannot be empty.");
            }

            await Add(course);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create course {course.Name}. Exception: {ex.Message}");
        }
    }

    public async Task DeleteCourse(int courseId)
    {
        try
        {
            var course = await GetById(courseId);
            
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            await Delete(course.Id);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete course. Exception: {ex.Message}");
        }
    }

    public async Task UpdateName(int courseId, string newName)
    {
        try
        {
            var course = await GetById(courseId);
            
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            course.Name = newName;

            await Update(course);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update course by {courseId} with new name: {newName}. Exception: {ex.Message}");
        }
    }
}