using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services;

public class UserCourseService : GenericService<UserCourses>, IUserCourseService
{
    private readonly IUserGroupService _userGroupService;
    public UserCourseService(UnitOfWork unitOfWork, IUserGroupService userGroupService) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.UserCoursesRepository;
        _userGroupService = userGroupService;
    }

    public async  Task AddTeacherToCourse(Course course, AppUser teacher)
    {
        try
        {
            if (course.Groups.Any())
            {
                foreach (var group in course.Groups)
                {
                    var userGroup = new UserGroups()
                    {
                        AppUser = teacher,
                        Group = group,
                    };
                    
                    await _userGroupService.CreateUserGroups(userGroup);
                }
            }
            
            var courseTeacher = new UserCourses()
            {
                Course = course,
                AppUser = teacher
            };

            await _repository.AddAsync(courseTeacher);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add teacher to course. Exception: {ex.Message}");
        }
    }

    public async Task CreateUserCourses(UserCourses userCourses)
    {
        try
        {
            await Add(userCourses);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create userCourses {userCourses.Id}. Exception: {ex.Message}");
        }
    }
}