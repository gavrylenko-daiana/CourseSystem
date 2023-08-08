using BLL.Interfaces;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services;

public class CourseService : GenericService<Course>, ICourseService
{
    private readonly IUserCourseService _userCourseService;
    
    public CourseService(UnitOfWork unitOfWork, IUserCourseService userCourseService) 
        : base(unitOfWork, unitOfWork.CourseRepository)
    {
        _userCourseService = userCourseService;
    }

    public async Task<Result<bool>> CreateCourse(Course course, AppUser currentUser)
    {
        if (course == null)
        {
            return new Result<bool>(false, $"{nameof(course)} not found");
        }
        
        if (currentUser == null)
        {
            return new Result<bool>(false, $"{nameof(currentUser)} not found");
        }
        
        try
        {
            await _repository.AddAsync(course);
            await _unitOfWork.Save();
            
            var userCourse = new UserCourses()
            {
                Course = course,
                AppUser = currentUser,
            };
            
            var createUserCoursesResult = await _userCourseService.CreateUserCourses(userCourse);
            
            if (!createUserCoursesResult.IsSuccessful)
            {
                await _repository.DeleteAsync(course);
                await _unitOfWork.Save();
            }
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,$"Failed to create {nameof(course)} {course.Id}. Exception: {ex.Message}");
        }
    }

    public async Task DeleteCourse(int courseId)
    {
        try
        {
            var course = await _repository.GetByIdAsync(courseId);
            
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            await _repository.DeleteAsync(course);
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
            var course = await _repository.GetByIdAsync(courseId);
            
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            course.Name = newName;

            await _repository.UpdateAsync(course);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update course by {courseId} with new name: {newName}. Exception: {ex.Message}");
        }
    }
}