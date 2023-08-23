using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface ICourseService : IGenericService<Course>
{
    Task<Result<bool>> CreateCourse(Course course, AppUser currentUser, IFormFile uploadFile);
    Task<Result<bool>> DeleteCourse(int courseId);
    Task<Result<bool>> UpdateName(int courseId, string newName);
    Task<Result<bool>> UpdateBackground(int courseId, IFormFile uploadFile);
    Task<Result<bool>> UpdateCourse(Course course);
    Task<Result<List<Course>>> GetUserCourses(AppUser currentUser, SortingParam sortOrder, string searchQuery = null);
    Task<Result<List<Course>>> GetAllCoursesAsync();
    Task<Result<bool>> AddEducationMaterial(DateTime uploadedTime, IFormFile uploadFile, MaterialAccess materialAccess,
        int? groupId = null, int? courseId = null);
    Task<Result<bool>> AddNewAdminToCourses(AppUser admin);
}