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

    public async Task<Result<bool>> DeleteCourse(int courseId)
    {
        var course = await _repository.GetByIdAsync(courseId);
        
        if (course == null)
        {
            return new Result<bool>(false, $"{nameof(course)} by id {courseId} not found");
        }
        
        try
        {
            await _repository.DeleteAsync(course);
            await _unitOfWork.Save();
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,$"Failed to delete {nameof(course)} by {courseId}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateCourse(Course course)
    {
        if (course == null)
        {
            return new Result<bool>(false, $"{nameof(course)} was not found");
        }
        
        try
        {
            await _repository.UpdateAsync(course);
            await _unitOfWork.Save();
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,$"Failed to update {nameof(course)}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateName(int courseId, string newName)
    {
        var course = await _repository.GetByIdAsync(courseId);
        
        if (course == null)
        {
            return new Result<bool>(false, $"{nameof(course)} by id {courseId} not found");
        }
        
        try
        {
            course.Name = newName;

            await _repository.UpdateAsync(course);
            await _unitOfWork.Save();
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,$"Failed to update {nameof(course)} by {courseId} with {newName}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<List<Course>>> GetAllCoursesAsync()
    {
        var courses = await _repository.GetAllAsync();

        if (!courses.Any())
        {
            return new Result<List<Course>>(false, "Course list is empty");
        }

        return new Result<List<Course>>(true, courses);
    }
}