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

    public async Task<Result<bool>> AddTeacherToCourse(Course course, AppUser teacher)
    {
        if (course == null)
        {
            return new Result<bool>(false, $"{nameof(course)} not found");
        }

        if (teacher == null)
        {
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

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to  add teacher to course. Exception: {ex.Message}");
        }
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

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to add student in group and course {userGroups.Id}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CreateUserCourses(UserCourses userCourses)
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
            return new Result<bool>(false, $"Failed to create {nameof(userCourses)} {userCourses.Id}. Exception: {ex.Message}");
        }
    }
}