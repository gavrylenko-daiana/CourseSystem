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

    public async Task<Result<bool>> AddTeacherToCourse(Course course, AppUser teacher)
    {
        if (course == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

            return new Result<bool>(false, $"{nameof(course)} not found");
        }

        if (teacher == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(teacher));

            return new Result<bool>(false, $"{nameof(teacher)} not found");
        }

        try
        {
            var courseTeacher = new UserCourses()
            {
                Course = course,
                AppUser = teacher
            };

            await _repository.AddAsync(courseTeacher);
            await _unitOfWork.Save();

            _logger.LogInformation("Successfully {action} in course {courseName} for user {userId}",
                MethodBase.GetCurrentMethod()?.Name, course.Name, teacher.Id);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} in course {courseName} for user {userId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, course.Name, teacher.Id, ex.Message);
            
            return new Result<bool>(false, $"Failed to  add teacher to course. Exception: {ex.Message}");
        }
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

            var userCourses = new UserCourses()
            {
                AppUser = userGroups.AppUser,
                Course = userGroups.Group.Course
            };

            var createUserCoursesResult = await CreateUserCourses(userCourses);

            if (!createUserCoursesResult.IsSuccessful)
            {
                return new Result<bool>(false, $"{createUserCoursesResult.Message}");
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

    public async Task<Result<bool>> CreateUserCourses(UserCourses userCourses)
    {
        if (userCourses == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(userCourses));

            return new Result<bool>(false, $"{nameof(userCourses)} not found");
        }

        try
        {
            await _repository.AddAsync(userCourses);
            await _unitOfWork.Save();

            _logger.LogInformation("Successfully {action} in course {courseName} for user {userId}",
                MethodBase.GetCurrentMethod()?.Name, userCourses.Course.Name, userCourses.AppUser.Id);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} in course {courseName} for user {userId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, userCourses.Course.Name, userCourses.AppUser.Id, ex.Message);
            
            return new Result<bool>(false, $"Failed to create {nameof(userCourses)} {userCourses.Id}. Exception: {ex.Message}");
        }
    }
}