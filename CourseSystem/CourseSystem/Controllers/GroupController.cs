using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.EmailTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using UI.ViewModels;
using Group = Core.Models.Group;
using Microsoft.AspNetCore.Cors.Infrastructure;
using UI.ViewModels.GroupViewModels;
using X.PagedList;
using System.Drawing.Printing;
using System.Linq;

namespace UI.Controllers;

[Authorize]
public class GroupController : Controller
{
    private readonly IGroupService _groupService;
    private readonly ICourseService _courseService;
    private readonly IEmailService _emailService;
    private readonly IUserGroupService _userGroupService;
    private readonly IUserCourseService _userCourseService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserService _userService;
    private readonly IActivityService _activityService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<GroupController> _logger;

    public GroupController(IGroupService groupService, ICourseService courseService,
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IUserGroupService userGroupService,
        IUserCourseService userCourseService,
        IUserService userService,
        IActivityService activityService,
        INotificationService notificationService,
        ILogger<GroupController> logger)
    {
        _groupService = groupService;
        _courseService = courseService;
        _emailService = emailService;
        _userManager = userManager;
        _userGroupService = userGroupService;
        _userCourseService = userCourseService;
        _userService = userService;
        _activityService = activityService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string currentQueryFilter, string currentAccessFilter, SortingParam sortOrder, string searchQuery, string groupAccessFilter, int? page)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }
        
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParam = sortOrder == SortingParam.NameDesc ? SortingParam.Name : SortingParam.NameDesc;
        ViewBag.StartDateParam = sortOrder == SortingParam.StartDateDesc ? SortingParam.StartDate : SortingParam.StartDateDesc;
        ViewBag.EndDateParam = sortOrder == SortingParam.EndDateDesc ? SortingParam.EndDate : SortingParam.EndDateDesc;

        if (searchQuery != null || groupAccessFilter != null)
        {
            page = 1;
        }
        else
        {
            searchQuery = currentQueryFilter;
            groupAccessFilter = currentAccessFilter;
        }

        ViewBag.CurrentQueryFilter = searchQuery;
        ViewBag.CurrentAccessFilter = groupAccessFilter;

        var groupsResult = await _groupService.GetUserGroups(currentUserResult.Data, sortOrder, groupAccessFilter, searchQuery);

        if (!groupsResult.IsSuccessful)
        {
            _logger.LogError("Groups fail for user {userId}! Error: {errorMessage}",
                currentUserResult.Data.Id, groupsResult.Message);
            TempData.TempDataMessage("Error", $"{groupsResult.Message}");

            return View("Index");
        }

        var groupsViewModels = groupsResult.Data.Select(group =>
        {
            var groupViewModel = new GroupViewModel();
            group.MapTo(groupViewModel);

            if (currentUserResult.Data.Role == AppUserRoles.Student)
            {
                var setProgressResult = _groupService.CalculateStudentProgressInGroup(group, currentUserResult.Data).Result;
                groupViewModel.Progress = setProgressResult;
            }
            else
            {
                groupViewModel.Progress = _groupService.CalculateGroupProgress(group.Id).Result;
            }
            
            groupViewModel.Progress = _groupService.CalculateGroupProgress(group.Id).Result;

            return groupViewModel;
        }).ToList();
        
        int pageSize = 6;
        int pageNumber = (page ?? 1);
        ViewBag.OnePageOfAssignemnts = groupsViewModels;
        
        return View(groupsViewModels.ToPagedList(pageNumber, pageSize));
    }

    [HttpGet]
    public async Task<IActionResult> Create(int courseId)
    {
        var courseResult = await _courseService.GetById(courseId);
        
        if (!courseResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{courseResult.Message}");
            return View("Index");
        }
        
        var groupViewModel = new GroupViewModel
        {
            CourseId = courseResult.Data.Id,
        };

        return View(groupViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(GroupViewModel groupViewModel)
    {
        var courseResult = await _courseService.GetById(groupViewModel.CourseId);
        
        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Course Id wasn't given");
            TempData.TempDataMessage("Error", $"{courseResult.Message}");

            return View("Index");
        }

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var group = new Group();
        groupViewModel.MapTo(group);
        
        group.Course = courseResult.Data;
        group.CourseId = groupViewModel.CourseId;

        var createResult = await _groupService.CreateGroup(group, currentUserResult.Data);

        if (!createResult.IsSuccessful)
        {
            _logger.LogError("Group creation fail for user {userId}! Error: {errorMessage}",
                currentUserResult.Data.Id, createResult.Message);
            TempData.TempDataMessage("Error", $"{createResult.Message}");

            return View(groupViewModel);
        }

        await _activityService.AddCreatedGroupActivity(currentUserResult.Data, group);

        await _notificationService.AddCreatedGroupNotification(currentUserResult.Data, group);

        return RedirectToAction("Details", "Group", new { id = group.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var groupResult = await _groupService.GetById(id);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                id, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }
        
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var userGroupVM = new UserGroupViewModel()
        {
            Group = groupResult.Data,
            UserGroupsWithoutAdmins = groupResult.Data.UserGroups.Where(ug => ug.AppUser.Role != AppUserRoles.Admin).ToList(),
            CurrentUser = currentUserResult.Data
        };
        
        userGroupVM.Progress = currentUserResult.Data.Role == AppUserRoles.Student ? 
            _groupService.CalculateStudentProgressInGroup(groupResult.Data, currentUserResult.Data).Result 
            : _groupService.CalculateGroupProgress(groupResult.Data.Id).Result;

        return View(userGroupVM);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var groupResult = await _groupService.GetById(id);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                id, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var groupViewModel = new GroupViewModel();
        groupResult.Data.MapTo(groupViewModel);

        return View(groupViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(GroupViewModel newGroupViewModel)
    {
        var newGroup = new Group();
        newGroupViewModel.MapTo(newGroup);

        var updateResult = await _groupService.UpdateGroup(newGroup);

        if (!updateResult.IsSuccessful)
        {
            _logger.LogError("Failed to update group by Id {groupId}! Error: {errorMessage}",
                newGroupViewModel.Id, updateResult.Message);
            TempData.TempDataMessage("Error", $"{updateResult.Message}");

            return View(newGroupViewModel);
        }

        return RedirectToAction("Details", new { id = newGroup.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var groupResult = await _groupService.GetById(id);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                id, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        return View(groupResult.Data);
    }

    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleteResult = await _groupService.DeleteGroup(id);

        if (!deleteResult.IsSuccessful)
        {
            _logger.LogError("Failed to delete group by Id {groupId}! Error: {errorMessage}",
                id, deleteResult.Message);
            TempData.TempDataMessage("Error", $"{deleteResult.Message}");

            return View("Delete");
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> SelectStudent(int id, int? page, bool approved = false)
    {
        var groupResult = await _groupService.GetById(id);
        
        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                id, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var students = await _userManager.GetUsersInRoleAsync("Student");
        var studentsInGroupIds = groupResult.Data.UserGroups.Select(ug => ug.AppUserId);
        
        var availableStudents = students.Where(s => !studentsInGroupIds.Contains(s.Id))
            .Select(u => new UserSelectionViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsSelected = false
            })
            .ToList();
        
        ViewBag.GroupId = id;
        ViewBag.Approved = approved;
        ViewBag.Students = availableStudents;
        
        int pageSize = 9;
        int pageNumber = (page ?? 1);
        ViewBag.OnePageOfAssignemnts = availableStudents;

        return View(availableStudents.ToPagedList(pageNumber, pageSize));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmSelection(int groupId, List<UserSelectionViewModel> students)
    {
        var selectedStudents = students.Where(s => s.IsSelected).ToList();
        var groupResult = await _groupService.GetById(groupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        if (selectedStudents.Count > 20)
        {
            _logger.LogInformation("More than 20 students added to group {groupId}!", groupId);
            TempData.TempDataMessage("Error", "Group cannot be more than 20 students without admin confirmation");

            return View("GetApprove", groupId);
        }
        else
        {
            var studentIds = selectedStudents.Select(s => s.Id).ToList();
            var studentsData = new Dictionary<string, string>();
            var callBacks = new List<string>();

            foreach (var studentId in studentIds)
            {
                var studentResult = await _userService.FindByIdAsync(studentId);

                if (!studentResult.IsSuccessful)
                {
                    _logger.LogError("Failed to get user by Id {userId}! Error: {errorMessage}",
                        studentId, studentResult.Message);

                    continue;
                    //Temporary solution
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(studentResult.Data);
                
                var callbackUrl = Url.Action(
                    "InvitationToGroup",
                    "Group",
                    new { groupId = groupId, code = code },
                    protocol: HttpContext.Request.Scheme);
                
                callBacks.Add(callbackUrl);
                studentsData.Add(studentResult.Data.Email, callbackUrl);
            }

            var result = await _emailService.SendInvitationToStudents(studentsData, groupResult.Data);

            if (!result.IsSuccessful)
            {
                _logger.LogError("Failed to send emails with invitation to group {groupId}! Error: {errorMessage}",
                    groupResult.Data.Id, result.Message);
                TempData.TempDataMessage("Error", result.Message);
            }
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> SelectTeachers(int courseId, int groupId, string? searchQuery, string? currentSearchQuery, int? page)
    {
        if (searchQuery != null)
        {
            page = 1;
        }
        else
        {
            searchQuery = currentSearchQuery;
        }
        
        ViewBag.CurrentQueryFilter = searchQuery!;
        
        var courseResult = await _courseService.GetById(courseId);

        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                courseId, courseResult.Message);
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");

            return View("Index");
        }

        var groupResult = await _groupService.GetById(groupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var teachersInCourse = courseResult.Data.UserCourses.Where(uc => uc.AppUser.Role == AppUserRoles.Teacher).Select(uc => uc.AppUser);
        var teachersNotInGroup = teachersInCourse.Except(groupResult.Data.UserGroups.Select(ug => ug.AppUser));

        var teachersViewModels = teachersInCourse
            .Select(u => new TeacherViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                ProfileImage = u.ProfileImage,
                IsInvited = !teachersNotInGroup.Any(teacher => teacher.Id == u.Id),
                CourseId = courseId,
                GroupId = groupId
            })
            .ToList();
        
        if (searchQuery != null)
        {
            teachersViewModels = teachersViewModels.Where(t =>
                t.FirstName.ToLower().Contains(searchQuery.ToLower()) ||
                t.LastName.ToLower().Contains(searchQuery.ToLower())).ToList();
        }

        ViewBag.SelectTeacherGroupId = groupId;
        ViewBag.SelectTeacherCourseId = courseId;

        int pageSize = 9;
        int pageNumber = (page ?? 1);

        return View(teachersViewModels.ToPagedList(pageNumber, pageSize));
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmTeacherSelection(string teacherId, int groupId)
    {
        var groupResult = await _groupService.GetById(groupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var userResult = await _userService.FindByIdAsync(teacherId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get teacher by Id {teacherId}! Error: {errorMessage}",
                teacherId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var userGroup = new UserGroups
        {
            AppUser = userResult.Data,
            Group = groupResult.Data
        };

        await _userGroupService.CreateUserGroups(userGroup);

        await _activityService.AddJoinedGroupActivity(userResult.Data, groupResult.Data);

        await _notificationService.AddJoinedGroupNotification(userResult.Data, groupResult.Data);

        return RedirectToAction("SelectTeachers", new { groupId = groupId, courseId = groupResult.Data.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> ApprovedSelection(int groupId, List<UserSelectionViewModel> students)
    {
        var selectedStudents = students.Where(s => s.IsSelected).ToList();
        var groupResult = await _groupService.GetById(groupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var studentIds = selectedStudents.Select(s => s.Id).ToList();
        var studentsData = new Dictionary<string, string>();
        var callBacks = new List<string>();

        foreach (var studentId in studentIds)
        {
            var studentResult = await _userService.FindByIdAsync(studentId);

            if (!studentResult.IsSuccessful)
            {
                _logger.LogError("Failed to get user by Id {userId}! Error: {errorMessage}",
                    studentId, studentResult.Message);

                continue;
                //Temporary solution
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(studentResult.Data);
            
            var callbackUrl = Url.Action(
                "InvitationToGroup",
                "Group",
                new { groupId = groupId, code = code },
                protocol: HttpContext.Request.Scheme);
            
            callBacks.Add(callbackUrl);
            studentsData.Add(studentResult.Data.Email, code);
        }

        var result = await _emailService.SendInvitationToStudents(studentsData, groupResult.Data);

        if (!result.IsSuccessful)
        {
            _logger.LogError("Failed to send emails with invitation to group {groupId}! Error: {errorMessage}",
                groupResult.Data.Id, result.Message);
            TempData.TempDataMessage("Error", result.Message);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> InvitationToGroup(int groupId, string code)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var result = await _userManager.ConfirmEmailAsync(currentUserResult.Data, code);

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to confirm email with invitation to group {groupId} for user {userId}! Errors: {errorMessages}",
                groupId, currentUserResult.Data.Id, result.Errors);

            return View("Error");
        }

        var groupResult = await _groupService.GetById(groupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var userGroup = new UserGroups()
        {
            AppUserId = currentUserResult.Data.Id,
            GroupId = groupId
        };

        Course course = null;

        if(!currentUserResult.Data.UserCourses.Any(uc => uc.CourseId == groupResult.Data.CourseId))
        {
            var courseResult = await _courseService.GetById(groupResult.Data.CourseId);

            if (!courseResult.IsSuccessful)
            {
                _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                    groupResult.Data.CourseId, courseResult.Message);
                ViewData.ViewDataMessage("Error", $"{courseResult.Message}");

                return View("Index");
            }

            course = courseResult.Data;
        }

        var addStudentToGroupAndCourseResult = await _userCourseService.AddStudentToGroupAndCourse(userGroup);

        if (!addStudentToGroupAndCourseResult.IsSuccessful)
        {
            _logger.LogError("Failed to add student {studentId} to group {groupId}! Error: {errorMessage}",
                userGroup.AppUserId, userGroup.AppUserId, addStudentToGroupAndCourseResult.Message);
            TempData.TempDataMessage("Error", addStudentToGroupAndCourseResult.Message);

            return RedirectToAction("Index", "Home");
        }

        if(course != null)
        {
            await _activityService.AddJoinedCourseActivity(currentUserResult.Data, course);

            await _notificationService.AddJoinedCourseNotification(currentUserResult.Data, course);
        }
        
        await _activityService.AddJoinedGroupActivity(currentUserResult.Data, groupResult.Data);

        await _notificationService.AddJoinedGroupNotification(currentUserResult.Data, groupResult.Data);

        var inventationVM = new InventationViewModel()
        {
            GroupName = groupResult.Data.Name, UserName = currentUserResult.Data.UserName
        };

        return View(inventationVM);
    }

    [HttpGet]
    [Route("GetApprove/{id}")]
    public async Task<IActionResult> GetApprove(int id)
    {
        //send email to admin for getting approve
        var groupResult = await _groupService.GetById(id);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                id, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var currentTeacherResult = await _userService.GetCurrentUser(User);

        if (!currentTeacherResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var callbackUrl = Url.Action(
            "AdminApprove",
            "Group",
            new { groupId = id, teacherId = currentTeacherResult.Data.Id },
            protocol: HttpContext.Request.Scheme);

        var sendEmailResult = await _emailService.SendEmailToAppUsers(EmailType.GroupConfirmationByAdmin, currentTeacherResult.Data, callbackUrl, group: groupResult.Data);

        if (!sendEmailResult.IsSuccessful)
        {
            _logger.LogError("Failed to send emails with confirmation of group {groupId}! Error: {errorMessage}",
                groupResult.Data.Id, sendEmailResult.Message);
            TempData.TempDataMessage("Error", sendEmailResult.Message);

            return RedirectToAction("Index", "Home");
        }

        //TempData.TempDataMessage("Error", sendEmailResult.Message);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminApprove(int groupId, string teacherId)
    {
        var groupResult = await _groupService.GetById(groupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        var teacherResult = await _userService.FindByIdAsync(teacherId);

        if (!teacherResult.IsSuccessful)
        {
            _logger.LogError("Failed to get user by Id {userId}!", teacherId);

            return View("Error");
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(teacherResult.Data);
        
        var callbackUrl = Url.Action(
            "ApprovedGroup",
            "Group",
            new { groupId = groupId, code = code },
            protocol: HttpContext.Request.Scheme);

        var sendEmailResult = await _emailService.SendEmailToAppUsers(EmailType.ApprovedGroupCreation, teacherResult.Data, callbackUrl, group: groupResult.Data);

        if (!sendEmailResult.IsSuccessful)
        {
            _logger.LogError("Failed to send emails with approvement of group {groupId}! Error: {errorMessage}",
                groupResult.Data.Id, sendEmailResult.Message);
        }

        return View();
    }


    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ApprovedGroup(int groupId, string code)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var result = await _userManager.ConfirmEmailAsync(currentUserResult.Data, code);

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to confirm email for user {userId} with code {userCode}! Errors: {errorMessages}",
                currentUserResult.Data.Id, code, result.Errors);

            return View("Error");
        }

        var groupResult = await _groupService.GetById(groupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");

            return View("Index");
        }

        return RedirectToAction("SelectStudent", "Group", new { id = groupId, approved = true });
    }

    
    [HttpGet]
    public async Task<IActionResult> DeleteUserFromGroup(int groupId, string userId)
    {
        var userResult = await _userService.FindByIdAsync(userId);

        if (!userResult.IsSuccessful)
        {
            _logger.LogWarning("Not found User");
            ViewData.ViewDataMessage("Error", $"{userResult.Message}");

            return View("Index");
        }
        
        var groupResult = await _groupService.GetById(groupId);
        
        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);
            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");
        
            return View("Index");
        }

        var userGroupViewModel = new UserGroupViewModel()
        {
            CurrentUser = userResult.Data,
            Group = groupResult.Data
        };
        
        return View(userGroupViewModel);
    }

    [HttpPost]
    [ActionName("DeleteUserFromGroup")]
    public async Task<IActionResult> DeleteUserFromGroupConfirmed(int groupId, string userId)
    {
        var userResult = await _userService.FindByIdAsync(userId);
        var groupResult = await _groupService.GetById(groupId);
        
        var deleteResult = await _groupService.DeleteUserFromGroup(groupResult.Data, userResult.Data);
        
        if (!deleteResult.IsSuccessful)
        {
            _logger.LogError("Failed to delete group by Id {groupId}! Error: {errorMessage}",
                groupId, deleteResult.Message);
            TempData.TempDataMessage("Error", $"{deleteResult.Message}");
        
            return View("DeleteUserFromGroup");
        }
        
        return RedirectToAction("Index");
    }
}