using System.Reflection;
using BLL.Interfaces;
using Core.Models;
using DAL.Repository;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class UserCourseService : GenericService<UserCourses>, IUserCourseService
{
    private readonly IUserGroupService _userGroupService;
    private readonly ILogger<UserCourseService> _logger;

    public UserCourseService(UnitOfWork unitOfWork, IUserGroupService userGroupService, ILogger<UserCourseService> logger)
        : base(unitOfWork, unitOfWork.UserCoursesRepository)
    {
        _userGroupService = userGroupService;
        _logger = logger;
    }

    public async Task<Result<bool>> AddStudentToGroupAndCourse(UserGroups userGroups)
    {
        if (userGroups == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(userGroups));
         
            return new Result<bool>(false, $"{nameof(userGroups)} not found");
        }

        try
        {
            await _userGroupService.CreateUserGroups(userGroups);

            if (userGroups.Group.Course.UserCourses.Any(uc => uc.AppUserId == userGroups.AppUserId))
            {
                return new Result<bool>(true);
            }

            var createUserCoursesResult = await AddUserInCourse(userGroups.AppUser, userGroups.Group.Course);

            if (!createUserCoursesResult.IsSuccessful)
            {
                return new Result<bool>(false, $"{createUserCoursesResult.Message}");
            }

            _logger.LogInformation("Successfully {action} in course {courseName} for user {userId}",
                MethodBase.GetCurrentMethod()?.Name, course.Name, teacher.Id);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} in course {courseName} for user {userId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, userGroups.Course.Name, userGroups.AppUser.Id, ex.Message);
            
            return new Result<bool>(false, $"Failed to add teacher to course. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CreateUserCoursesForTests(UserCourses userCourses)
    {
        if (userCourses == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(userGroups));

            return new Result<bool>(false, $"{nameof(userGroups)} not found");
        }

        try
        {
            await _repository.AddAsync(userCourses);
            await _unitOfWork.Save();

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,
                $"Failed to create {nameof(userCourses)} {userCourses.Id}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddUserInCourse(AppUser appUser, Course course)
    {
        if (appUser == null)
        {
            return new Result<bool>(false, $"{nameof(appUser)} not found");
        }

        if (course == null)
        {
            return new Result<bool>(false, $"{nameof(course)} not found");
        }

        var userCourses = new UserCourses
        {
            AppUser = appUser,
            AppUserId = appUser.Id,
            Course = course,
            CourseId = course.Id
        };

        try
        {
            if (!await IsUserInCourse(userCourses))
            {
                await _repository.AddAsync(userCourses);
                await _unitOfWork.Save();
            }

            _logger.LogInformation("Successfully {action} in group {groupName} for user {userId}",
                MethodBase.GetCurrentMethod()?.Name, userGroups.Group.Name, userGroups.AppUser.Id);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} in group {groupName} for user {userId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, userGroups.Group.Name, userGroups.AppUser.Id, ex.Message);
            
            return new Result<bool>(false, $"Failed to add student in group and course {userGroups.Id}. Exception: {ex.Message}");
        }
    }

    private async Task<bool> IsUserInCourse(UserCourses userCourses)
    {
        var userCourse = await GetUserCourses(userCourses.AppUserId, userCourses.CourseId);

        return userCourse.Data != null;
    }

    private async Task<Result<UserCourses>> GetUserCourses(string? userId, int courseId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(userId));
            
            return new Result<UserCourses>(false, $"{nameof(userId)} not found");
        }

        if (courseId < 1)
        {
            return new Result<UserCourses>(false, $"{nameof(courseId)} not found");
        }

        try
        {
            var userCourses = await _repository.GetAsync();
            var getUserCourse = userCourses.FirstOrDefault(uc => uc.AppUserId == userId && uc.CourseId == courseId);
            
            _logger.LogInformation("Successfully {action} in course {courseId} for user {userId}",
                MethodBase.GetCurrentMethod()?.Name, courseId, userId);
                
            return new Result<UserCourses>(true, getUserCourse!);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} in course {courseId} for user {userId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, courseId, userId, ex.Message);
                
            return new Result<UserCourses>(false, $"Message - {ex.Message}");
        }
    }
}