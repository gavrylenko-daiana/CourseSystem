using System.Linq.Expressions;
using System.Reflection;
using BLL.Interfaces;
using Core.Configuration;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace BLL.Services;

public class CourseService : GenericService<Course>, ICourseService
{
    private readonly IUserCourseService _userCourseService;
    private readonly IEducationMaterialService _educationMaterialService;
    private readonly IDropboxService _dropboxService;
    private readonly IGroupService _groupService;
    private readonly ILogger<CourseService> _logger;

    public CourseService(UnitOfWork unitOfWork, IUserCourseService userCourseService, IEducationMaterialService educationMaterial, 
        IGroupService groupService, IDropboxService dropboxService, ILogger<CourseService> logger)
        : base(unitOfWork, unitOfWork.CourseRepository)
    {
        _userCourseService = userCourseService;
        _educationMaterialService = educationMaterial;
        _groupService = groupService;
        _dropboxService = dropboxService;
        _logger = logger;
    }

    public async Task<Result<bool>> CreateCourse(Course course, AppUser currentUser, IFormFile uploadFile)
    {
        if (course == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));
            
            return new Result<bool>(false, $"{nameof(course)} not found");
        }

        if (currentUser == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(currentUser));

            return new Result<bool>(false, $"{nameof(currentUser)} not found");
        }

        try
        {
            var getUrlResult = await GetBackgroundImageUrl(uploadFile);
            
            if (!getUrlResult.IsSuccessful)
            {
                return new Result<bool>(false, $"{getUrlResult.Message}");
            }
            
            course.Url = getUrlResult.Data;

            await _repository.AddAsync(course);
            await _unitOfWork.Save();
            
            _logger.LogInformation("Successfully {action} with {courseName} with user by {userId}",
                MethodBase.GetCurrentMethod()?.Name, course.Name, currentUser.Id);

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
                
                _logger.LogInformation("Successfully delete course after unsuccessfully create userCourse with user by {userId}", currentUser.Id);
            }

            _logger.LogInformation("Successfully {action} and add userCourse with {courseName} with user by {userId}",
                MethodBase.GetCurrentMethod()?.Name, course.Name, currentUser.Id);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} and add userCourse with {courseName} with user by {userId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, course.Name, currentUser.Id, ex.Message);
            
            return new Result<bool>(false, $"Failed to create {nameof(course)} {course.Id}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteCourse(int courseId)
    {
        var course = await _repository.GetByIdAsync(courseId);

        if (course == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

            return new Result<bool>(false, $"{nameof(course)} by id {courseId} not found");
        }

        foreach (var notification in course.Notifications)
        {
            notification.AssignmentId = null;
            notification.GroupId = null;
            notification.CourseId = null;
        }

        try
        {
            if (course.EducationMaterials.Any())
            {
                var educationMaterialsCopy = course.EducationMaterials.ToList();
            
                foreach (var material in educationMaterialsCopy)
                {
                    await _educationMaterialService.DeleteFile(material);
                }
            }

            //delete background

            await _repository.DeleteAsync(course);
            await _unitOfWork.Save();

            _logger.LogInformation("Successfully {action} by id {entityId}",
                MethodBase.GetCurrentMethod()?.Name, courseId);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} by id {entityId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, courseId, ex.Message);
            
            return new Result<bool>(false, $"Failed to delete {nameof(course)} by {courseId}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateCourse(Course course)
    {
        if (course == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

            return new Result<bool>(false, $"{nameof(course)} was not found");
        }

        try
        {
            await _repository.UpdateAsync(course);
            await _unitOfWork.Save();

            _logger.LogInformation("Successfully {action} with {entityName}",
                MethodBase.GetCurrentMethod()?.Name, course.Name);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} with {entityName}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, course.Name, ex.Message);
            
            return new Result<bool>(false, $"Failed to update {nameof(course)}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<List<Course>>> GetUserCourses(AppUser currentUser, SortingParam sortOrder, string searchQuery = null)
    {
        if (currentUser == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(currentUser));

            return new Result<List<Course>>(false, $"{nameof(currentUser)} not found");
        }
        
        Result<List<Course>> coursesResult = null;

        var courses = currentUser.UserCourses.Select(uc => uc.Course).ToList();

        if (!courses.Any())
        {
            _logger.LogInformation("{listName} is empty", nameof(courses));
            
            return new Result<List<Course>>(true, new List<Course>());
        }

        var backgroundCheckResult = await CheckIsBackgroundExist(courses); //Return data is result for logger message
        
        var query = GetOrderByExpression(sortOrder);
        
        if (!string.IsNullOrEmpty(searchQuery))
        {
            coursesResult = await GetByPredicate(c => c.Name.Contains(searchQuery) && courses.Contains(c), query);
        }
        else
        {
            coursesResult = await GetByPredicate(c => courses.Contains(c), query);
        }
        
        return coursesResult;
    }

    public async Task<Result<bool>> UpdateName(int courseId, string newName)
    {
        var course = await _repository.GetByIdAsync(courseId);

        if (course == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

            return new Result<bool>(false, $"{nameof(course)} by id {courseId} not found");
        }

        try
        {
            course.Name = newName;

            await _repository.UpdateAsync(course);
            await _unitOfWork.Save();
            
            _logger.LogInformation("Successfully {action} on {entityName}",
                MethodBase.GetCurrentMethod()?.Name, course.Name);

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} on {entityName}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, course.Name, ex.Message);
            
            return new Result<bool>(false,
                $"Failed to update {nameof(course)} by {courseId} with {newName}. Exception: {ex.Message}");
        }
    }
    
    public async Task<Result<bool>> UpdateBackground(int courseId, IFormFile uploadFile)
    {
        var course = await _repository.GetByIdAsync(courseId);

        if (course == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

            return new Result<bool>(false, $"{nameof(course)} by id {courseId} not found");
        }

        try
        {
            var getUrlResult = await GetBackgroundImageUrl(uploadFile);
            
            if (!getUrlResult.IsSuccessful)
            {
                return new Result<bool>(false, $"{getUrlResult.Message}");
            }
            
            course.Url = getUrlResult.Data;

            await _repository.UpdateAsync(course);
            await _unitOfWork.Save();
            
            _logger.LogInformation("Successfully {action} for {courseName} with {fileName}",
                MethodBase.GetCurrentMethod()?.Name, course.Name, uploadFile.Name);

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} for {courseName} with {fileName}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, course.Name, uploadFile.Name, ex.Message);
            
            return new Result<bool>(false, $"Failed to update {nameof(course)} by {courseId} with {uploadFile.Name}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<List<Course>>> GetAllCoursesAsync()
    {
        var courses = await _repository.GetAllAsync();

        if (!courses.Any())
        {
            return new Result<List<Course>>(false, "Course list is empty");
        }

        _logger.LogInformation("Successfully {action}", MethodBase.GetCurrentMethod()?.Name);
        
        return new Result<List<Course>>(true, courses);
    }
    
    public async Task<Result<bool>> AddEducationMaterial(DateTime uploadTime, IFormFile uploadFile, MaterialAccess materialAccess,
        int? groupId = null, int? courseId = null)
    {
        var fullPath = await _dropboxService.AddFileAsync(uploadFile, materialAccess.ToString());

        if (!fullPath.IsSuccessful)
        {
            return new Result<bool>(false, $"Failed to upload file: {fullPath.Message}");
        }

        _logger.LogInformation("Material access for {materialAccess}", materialAccess);
        
        if (materialAccess == MaterialAccess.Group && groupId.HasValue)
        {
            var groupResult = await _groupService.GetById(groupId.Value);

            if (!groupResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Message: {groupResult.Message}");
            }

            var addToGroupResult = await _educationMaterialService.AddEducationMaterial(uploadTime, fullPath.Data.ModifiedFileName, fullPath.Data.Url,
                MaterialAccess.Group, groupResult.Data);

            if (!addToGroupResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Message: {addToGroupResult.Message}");
            }
        }
        else if (materialAccess == MaterialAccess.Course && courseId.HasValue)
        {
            var courseResult = await GetById(courseId.Value);

            if (!courseResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Message: {courseResult.Message}");
            }

            var addToCourseResult = await _educationMaterialService.AddEducationMaterial(uploadTime, fullPath.Data.ModifiedFileName, fullPath.Data.Url,
                MaterialAccess.Course, null!, courseResult.Data);

            if (!addToCourseResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Message: {addToCourseResult.Message}");
            }
        }
        else if (materialAccess == MaterialAccess.General)
        {
            var addToCourseResult =
                await _educationMaterialService.AddEducationMaterial(uploadTime, fullPath.Data.ModifiedFileName, fullPath.Data.Url,
                    MaterialAccess.General);

            if (!addToCourseResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Message: {addToCourseResult.Message}");
            }
        }

        _logger.LogInformation("Successfully {action} with {fileName}",
            MethodBase.GetCurrentMethod()?.Name, uploadFile.Name);
        
        return new Result<bool>(true);
    }
    
    private Expression<Func<IQueryable<Course>, IOrderedQueryable<Course>>> GetOrderByExpression(SortingParam sortBy)
    {
        Expression<Func<IQueryable<Course>, IOrderedQueryable<Course>>> query;

        switch (sortBy)
        {
            case SortingParam.NameDesc:
                query = q => q.OrderByDescending(q => q.Name);
                break;
            default:
                query = q => q.OrderBy(q => q.Name);
                break;
        }

        _logger.LogInformation("Successfully {action}, query: {query}",
            MethodBase.GetCurrentMethod()?.Name, query.ToString());
        
        return query;
    }
    
    private async Task<Result<string>> GetRandomDefaultBackgroundLink()
    {
        var defaultImagesLinks = DefaultBackgroundImages.GetDefaultImagesLinks();

        if (defaultImagesLinks.Count > 0)
        {
            var randomIndex = new Random().Next(0, defaultImagesLinks.Count);
            var randomImageLink = defaultImagesLinks[randomIndex];

            _logger.LogInformation("Successfully {action} with random index {randomIndex} - link {randomLink}",
                MethodBase.GetCurrentMethod()?.Name, randomIndex, randomImageLink);
            
            return new Result<string>(true, data: randomImageLink);
        }
        else
        {
            _logger.LogError("Failed to {action}. No default images available", MethodBase.GetCurrentMethod()?.Name);
            
            return new Result<string>(false, "No default images available");
        }
    }

    private async Task<Result<string>> GetBackgroundImageUrl(IFormFile uploadFile)
    {
        if (uploadFile == null)
        {
            var backgroundUrlResult = await GetRandomDefaultBackgroundLink();

            if (!backgroundUrlResult.IsSuccessful)
            {
                return new Result<string>(false, $"{backgroundUrlResult.Message}");
            }

            return new Result<string>(true, data: backgroundUrlResult.Data);
        }
        else
        {
            var backgroundUrlResult = await _dropboxService.AddFileAsync(uploadFile, DropboxFolders.CourseBackgroundImages.ToString());
                
            if (!backgroundUrlResult.IsSuccessful)
            {
                return new Result<string>(false, $"{backgroundUrlResult.Message}");
            }
                
            return new Result<string>(true, data: backgroundUrlResult.Data.Url);
        }
    }

    private async Task<Result<bool>> CheckIsBackgroundExist(List<Course> courses)
    {
        if (courses == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(courses));

            return new Result<bool>(false, $"Invalid input {nameof(courses)}");
        }

        if (!courses.Any())
        {
            return new Result<bool>(true, $"No {nameof(courses)} for background updating");
        }

        var coursesWithoutBackgroundImage = courses.Where(c => c.Url.IsNullOrEmpty()).ToList();

        if (!coursesWithoutBackgroundImage.Any())
        {
            return new Result<bool>(true, $"Successful {nameof(courses)} background updating");
        }

        try
        {
            foreach (var course in coursesWithoutBackgroundImage)
            {
                var randomBackgroundResult = await GetRandomDefaultBackgroundLink();
                course.Url = randomBackgroundResult.Data;

                await _repository.UpdateAsync(course);
            }

            await _unitOfWork.Save();
            
            _logger.LogInformation("Successfully {action}", MethodBase.GetCurrentMethod()?.Name);

            return new Result<bool>(true, $"Successful {nameof(courses)} background updating");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action}. Error: {errorMsg}", MethodBase.GetCurrentMethod()?.Name, ex.Message);
            
            return new Result<bool>(false, $"Fail to update {nameof(courses)} background.");
        }
    }
}