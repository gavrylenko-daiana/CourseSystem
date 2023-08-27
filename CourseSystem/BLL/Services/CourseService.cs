using System.Linq.Expressions;
using System.Reflection;
using BLL.Interfaces;
using Core.Configuration;
using Core.Enums;
using Core.ImageStore;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static System.Net.WebRequestMethods;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace BLL.Services;

public class CourseService : GenericService<Course>, ICourseService
{
    private readonly IUserCourseService _userCourseService;
    private readonly IEducationMaterialService _educationMaterialService;
    private readonly ICourseBackgroundImageService _courseBackgroundImageService;
    private readonly IDropboxService _dropboxService;
    private readonly IGroupService _groupService;
    private readonly ILogger<CourseService> _logger;
    private readonly UserManager<AppUser> _userManager;

    public CourseService(UnitOfWork unitOfWork, IUserCourseService userCourseService,
        IEducationMaterialService educationMaterial, ICourseBackgroundImageService courseBackgroundImageService,
        IGroupService groupService, IDropboxService dropboxService, 
        UserManager<AppUser> userManager, ILogger<CourseService> logger)
        : base(unitOfWork, unitOfWork.CourseRepository)
    {
        _userCourseService = userCourseService;
        _educationMaterialService = educationMaterial;
        _courseBackgroundImageService = courseBackgroundImageService;
        _groupService = groupService;
        _dropboxService = dropboxService;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<Result<bool>> CreateCourse(Course course, AppUser currentUser, IFormFile uploadFile = null)
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
            await _repository.AddAsync(course);

            if (uploadFile == null)
            {
                var setBackgroundResult = await _courseBackgroundImageService.SetDefaultBackgroundImage(course);

                if (!setBackgroundResult.IsSuccessful)
                {
                    _logger.LogError("Failed to {action}! Error: {ErrorMsg}",
                        MethodBase.GetCurrentMethod()?.Name, setBackgroundResult.Message);

                    await _repository.DeleteAsync(course);

                    return new Result<bool>(false, $"Failed to create {nameof(course)} - failed to add background image");
                }
            }
            else
            {
                var setBackgroundResult = await _courseBackgroundImageService.SetCustomBackgroundImage(course, uploadFile);

                if (!setBackgroundResult.IsSuccessful)
                {
                    _logger.LogError("Failed to {action}! Error: {ErrorMsg}",
                        MethodBase.GetCurrentMethod()?.Name, setBackgroundResult.Message);

                    await _repository.DeleteAsync(course);

                    return new Result<bool>(false, $"Failed to create {nameof(course)} - failed to add background image");
                }
            }
            
            _logger.LogInformation("Successfully {action} with {courseName} with user by {userId}",
                MethodBase.GetCurrentMethod()?.Name, course.Name, currentUser.Id);

            var admins = await _userManager.GetUsersInRoleAsync("Admin"); // course can be added only by Admin

            if (admins.Any())
            {
                foreach (var admin in admins)
                {
                    await _userCourseService.AddUserInCourse(admin, course);
                }
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

            var deleteBackgroundResult = await _courseBackgroundImageService.DeleteCourseBackgroundImage(course);

            if (!deleteBackgroundResult.IsSuccessful)
            {
                _logger.LogError("Failed to {action}! Error: {ErrorMsg}", 
                    MethodBase.GetCurrentMethod()?.Name, deleteBackgroundResult.Message);

                return new Result<bool>(false, $"Failed to delete {nameof(course)} by Id {courseId} - failed to delete background");
            }

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

    public async Task<Result<List<Course>>> GetUserCourses(AppUser currentUser, SortingParam sortOrder,
        string searchQuery = null)
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

        var backgroundCheckResult = await CheckIfBackgroundsExist(courses); //Return data is result for logger message

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
            var updateBackgroundResult = await _courseBackgroundImageService.UpdateBackgroundImage(course, uploadFile);

            if (!updateBackgroundResult.IsSuccessful)
            {
                _logger.LogError("Failed to {action}! Error: {ErrorMsg}",
                    MethodBase.GetCurrentMethod()?.Name, updateBackgroundResult.Message);

                return new Result<bool>(false, $"Failed to update {nameof(course)}");
            }

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
        var courses = await _repository.GetAsync();

        if (!courses.Any())
        {
            return new Result<List<Course>>(false, "Course list is empty");
        }

        _logger.LogInformation("Successfully {action}", MethodBase.GetCurrentMethod()?.Name);
        
        return new Result<List<Course>>(true, courses);
    }

    public async Task<Result<bool>> AddEducationMaterial(DateTime uploadTime, IFormFile uploadFile,
        MaterialAccess materialAccess,
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

            var addToGroupResult = await _educationMaterialService.AddEducationMaterial(uploadTime,
                fullPath.Data.ModifiedFileName, fullPath.Data.Url,
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

            var addToCourseResult = await _educationMaterialService.AddEducationMaterial(uploadTime,
                fullPath.Data.ModifiedFileName, fullPath.Data.Url,
                MaterialAccess.Course, null!, courseResult.Data);

            if (!addToCourseResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Message: {addToCourseResult.Message}");
            }
        }
        else if (materialAccess == MaterialAccess.General)
        {
            var addToCourseResult =
                await _educationMaterialService.AddEducationMaterial(uploadTime, fullPath.Data.ModifiedFileName,
                    fullPath.Data.Url,
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

        _logger.LogInformation("Successfully {action}, query: {query}", MethodBase.GetCurrentMethod()?.Name, query.ToString());
        
        return query;
    }

    private async Task<Result<bool>> CheckIfBackgroundsExist(List<Course> courses)
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

        var coursesWithoutBackgroundImage = courses.Where(c => c.BackgroundImage == null).ToList();

        if (!coursesWithoutBackgroundImage.Any())
        {
            return new Result<bool>(true, $"Successful {nameof(courses)} background updating");
        }

        try
        {
            foreach (var course in coursesWithoutBackgroundImage)
            {
                var setBackgroundResult = await _courseBackgroundImageService.SetDefaultBackgroundImage(course);

                if (!setBackgroundResult.IsSuccessful)
                {
                    _logger.LogError("Failed to {action}! Error: {ErrorMsg}",
                        MethodBase.GetCurrentMethod()?.Name, setBackgroundResult.Message);

                    return new Result<bool>(false, $"Fail to update {nameof(course)} {course.Id} background.");
                }
            }
            
            _logger.LogInformation("Successfully {action}", MethodBase.GetCurrentMethod()?.Name);

            return new Result<bool>(true, $"Successful {nameof(courses)} background updating");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action}. Error: {errorMsg}", MethodBase.GetCurrentMethod()?.Name, ex.Message);
            
            return new Result<bool>(false, $"Fail to update {nameof(courses)} background.");
        }
    }

    public async Task<Result<bool>> AddNewAdminToCourses(AppUser admin)
    {
        var courses = await _repository.GetAsync();

        if (courses != null)
        {
            foreach (var course in courses)
            {
                var userCoursesResult = await _userCourseService.AddUserInCourse(admin, course);

                if (!userCoursesResult.IsSuccessful)
                {
                    return new Result<bool>(false, userCoursesResult.Message);
                }

                if (course.Groups != null)
                {
                    foreach (var group in course.Groups)
                    {
                        var userGroupsResult = await _groupService.AddAllAdminsAtGroup(group);

                        if (!userGroupsResult.IsSuccessful)
                        {
                            return new Result<bool>(false, userGroupsResult.Message);
                        }
                    }
                }
            }
        }

        return new Result<bool>(true);
    }

    public async Task<Result<bool>> DeleteUserFromCourse(Course course, AppUser deletedUser)
    {
        if (course == null)
        {
            return new Result<bool>(false, $"{nameof(course)} not found");
        }

        if (deletedUser == null)
        {
            return new Result<bool>(false, $"{nameof(deletedUser)} not found");
        }

        try
        {
            foreach (var group in course.Groups)
            {
                if(group.UserGroups.Any(ug => ug.AppUserId == deletedUser.Id))
                {
                    await _groupService.DeleteUserFromGroup(group, deletedUser);
                }
            }

            foreach (var userCourse in course.UserCourses)
            {
                if (userCourse.AppUserId == deletedUser.Id)
                {
                    course.UserCourses.Remove(userCourse);

                    break;
                }
            }

            await _repository.UpdateAsync(course);
            await _unitOfWork.Save();

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to delete user {deletedUser.Id} from course {course.Id}. Exception: {ex.Message}");
        }
    }
}