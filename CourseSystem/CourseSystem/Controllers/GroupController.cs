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
    private readonly ILogger<GroupController> _logger;

    public GroupController(IGroupService groupService, ICourseService courseService,
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IUserGroupService userGroupService,
        IUserCourseService userCourseService,
        ILogger<GroupController> logger)
    {
        _groupService = groupService;
        _courseService = courseService;
        _emailService = emailService;
        _userManager = userManager;
        _userGroupService = userGroupService;
        _userCourseService = userCourseService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unouthirized user");
            return RedirectToAction("Login", "Account");
        }

        var groupsResult = await _groupService.GetByPredicate(g =>
            g.UserGroups.Any(ug => ug.AppUserId.Equals(currentUserResult.Data.Id)));
        
        if (!groupsResult.IsSuccessful)
        {
            _logger.LogError("Groups fail for user {userId}! Error: {errorMessage}",
                currentUserResult.Data.Id, groupsResult.Message);

            TempData.TempDataMessage("Error", $"{groupsResult.Message}");
            return View("Index");
        }
        
        var groupViewModels = groupsResult.Data.Select(group =>
        {
            var groupViewModel = new GroupViewModel();
            group.MapTo(groupViewModel);
            groupViewModel.Progress = _groupService.CalculateGroupProgress(group.Id).Result;
            
            return groupViewModel;
        }).ToList();

        var userGroupsViewModel = new UserGroupsViewModel()
        {
            Groups = groupViewModels,
            CurrentUser = currentUserResult.Data,
        };
        
        return View(userGroupsViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var groupViewModel = new GroupViewModel();
        
        return View(groupViewModel);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(GroupViewModel groupViewModel)
    {
        if (TempData["CourseId"] == null)
        {
            _logger.LogError("Course Id wasn't given");
            ViewData.ViewDataMessage("Error", "Course Id wasn't given");
            return View("Index");
        }

        var courseId = (int)TempData["CourseId"];
        var courseResult = await _courseService.GetById(courseId);
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unouthirized user");
            return RedirectToAction("Login", "Account");
        }

        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                courseId, courseResult.Message);
            TempData.TempDataMessage("Error", "Course not found");
            return View(groupViewModel);
        }

        var group = new Group();
        groupViewModel.MapTo(group);
        group.Course = courseResult.Data;
        group.CourseId = courseResult.Data.Id;

        var createResult = await _groupService.CreateGroup(group, currentUserResult.Data);

        if (!createResult.IsSuccessful)
        {
            _logger.LogError("Group creation fail for user {userId}! Error: {errorMessage}",
                currentUserResult.Data.Id, createResult.Message);

            TempData["CourseId"] = courseId;
            TempData.TempDataMessage("Error", $"{createResult.Message}");
            return View(groupViewModel);
        }
        
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
        
        TempData["GroupId"] = id;

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unouthirized user");
            return RedirectToAction("Login", "Account");
        }

        var userGroupVM = new UserGroupViewModel()
        {
            Group = groupResult.Data,
            UserGroupsWithoutAdmins = groupResult.Data.UserGroups.Where(ug => ug.AppUser.Role != AppUserRoles.Admin).ToList(),
            CurrentUser = currentUserResult.Data
        };

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
    public async Task<IActionResult> SelectStudent(int id, bool approved = false)
    {
        var groupId = (int)(TempData["GroupId"] ?? id);
        var groupResult = await _groupService.GetById(groupId);
        
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
        
        ViewBag.GroupId = groupId;
        ViewBag.Approved = approved;

        return View(availableStudents);
    }

    [HttpPost]
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
    public async Task<IActionResult> SelectTeachers(int courseId, int groupId)
    {
        var courseResult = await _courseService.GetById(courseId);
        var groupResult = await _groupService.GetById(groupId);
        
        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);

            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");
            return View("Index");
        }
        
        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                courseId, courseResult.Message);

            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");
            return View("Index");
        }
        
        var teachersInCourse = courseResult.Data.UserCourses.Where(uc => uc.AppUser.Role == AppUserRoles.Teacher).Select(uc => uc.AppUser);
        var teachersNotInGroup = teachersInCourse.Except(groupResult.Data.UserGroups.Select(ug => ug.AppUser));
        
        var teachersViewModels = teachersNotInGroup.Select(teacher => new UserSelectionViewModel
        {
            Id = teacher.Id,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            IsSelected = false
        }).ToList();

        ViewBag.GroupId = groupId;

        return View(teachersViewModels);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmTeachersSelection(int groupId, List<UserSelectionViewModel> teachers)
    {
        var selectedTeachersVM = teachers.Where(s => s.IsSelected).ToList();
        var selectedTeachersTasks = selectedTeachersVM.Select(s => s.Id).
            Select(id => _userManager.FindByIdAsync(id));
        var selectedTeachers = (await Task.WhenAll(selectedTeachersTasks)).ToList();
        var groupResult = await _groupService.GetById(groupId);
        
        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);

            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");
            return View("Index");
        }

        if (selectedTeachers != null && selectedTeachers.Count > 0)
        {
            foreach (var teacher in selectedTeachers)
            {
                var userGroup = new UserGroups
                {
                    AppUser = teacher,
                    Group = groupResult.Data
                };

                await _userGroupService.CreateUserGroups(userGroup);
            }
        }

        return RedirectToAction("Details", new { id = groupId });
    }
    
    [HttpPost]
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
            _logger.LogWarning("Unouthirized user");
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

        var addStudentToGroupAndCourseResult = await _userCourseService.AddStudentToGroupAndCourse(userGroup);
            
        if (!addStudentToGroupAndCourseResult.IsSuccessful)
        {
            _logger.LogError("Failed to add student {studentId} to group {groupId}! Error: {errorMessage}",
                userGroup.AppUserId, userGroup.AppUserId, addStudentToGroupAndCourseResult.Message);

            TempData.TempDataMessage("Error", addStudentToGroupAndCourseResult.Message);
            return RedirectToAction("Index", "Home");
        }

        var inventationVM = new InventationViewModel() { GroupName = groupResult.Data.Name, UserName = currentUserResult.Data.UserName};

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
            _logger.LogWarning("Unouthirized user");
            return RedirectToAction("Login", "Account");
        }
            
        var callbackUrl = Url.Action( 
            "AdminApprove",
            "Group",
            new { groupId = id, teacherId = currentTeacherResult.Data.Id },
            protocol: HttpContext.Request.Scheme);

        var sendEmailResult = await _emailService.SendEmailGroups(EmailType.GroupConfirmationByAdmin, groupResult.Data, callbackUrl);

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
        var teacherResult = await _userService.FindByIdAsync(teacherId);
        
        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);

            ViewData.ViewDataMessage("Error", $"{groupResult.Message}");
            return View("Index");
        }
        
        if (!teacherResult.IsSuccessful)
        {
            _logger.LogError("Failed to get user by Id {userId}!", teacherId);
            return View("Error");
        }
            
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(teacherResult.Data);
        var callbackUrl = Url.Action( 
           "ApprovedGroup",
           "Group",
           new { groupId = groupId, code = code},
           protocol: HttpContext.Request.Scheme);

        var sendEmailResult = await _emailService.SendEmailGroups(EmailType.ApprovedGroupCreation, groupResult.Data, callbackUrl, teacherResult.Data);

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
            _logger.LogWarning("Unouthirized user");
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

}