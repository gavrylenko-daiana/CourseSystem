using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services;

public class UserCourseService : GenericService<UserCourses>, IUserCourseService
{
    private readonly IUserGroupService _userGroupService;

    public UserCourseService(UnitOfWork unitOfWork, IUserGroupService userGroupService)
        : base(unitOfWork, unitOfWork.UserCoursesRepository)
    {
        _userGroupService = userGroupService;
    }

    public async Task<Result<bool>> AddStudentToGroupAndCourse(UserGroups userGroups)
    {
        if (userGroups == null)
        {
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

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,
                $"Failed to add student in group and course {userGroups.Id}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CreateUserCoursesForTests(UserCourses userCourses)
    {
        if (userCourses == null)
        {
            return new Result<bool>(false, $"{nameof(userCourses)} not found");
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

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,
                $"Failed to create {nameof(userCourses)} {userCourses.Id}. Exception: {ex.Message}");
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

            return new Result<UserCourses>(true, getUserCourse!);
        }
        catch (Exception ex)
        {
            return new Result<UserCourses>(false, $"Message - {ex.Message}");
        }
    }
}