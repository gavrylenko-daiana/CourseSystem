using BLL.Interfaces;
using Core.EmailTemplates;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using UI.ViewModels;
using UI.ViewModels.CourseViewModels;
using X.PagedList;

namespace UI.Controllers;

[Authorize]
public class CourseController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEmailService _emailService;
    private readonly IUserCourseService _userCourseService;
    private readonly IUserService _userService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IActivityService _activityService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CourseController> _logger;

    public CourseController(ICourseService courseService,
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IUserCourseService userCourseService,
        IUserService userService,
        IActivityService activityService,
        INotificationService notificationService,
        ILogger<CourseController> logger)
    {
        _courseService = courseService;
        _userManager = userManager;
        _emailService = emailService;
        _userCourseService = userCourseService;
        _userService = userService;
        _activityService = activityService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string currentQueryFilter, SortingParam sortOrder, string searchQuery,
        int? page)
    {
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParam = sortOrder == SortingParam.NameDesc ? SortingParam.Name : SortingParam.NameDesc;

        if (searchQuery != null)
        {
            page = 1;
        }
        else
        {
            searchQuery = currentQueryFilter;
        }

        ViewBag.CurrentQueryFilter = searchQuery;

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var coursesResult = await _courseService.GetUserCourses(currentUserResult.Data, sortOrder, searchQuery);

        if (!coursesResult.IsSuccessful)
        {
            _logger.LogError("Courses fail for user {userId}! Error: {errorMessage}",
                currentUserResult.Data.Id, coursesResult.Message);

            TempData.TempDataMessage("Error", $"{coursesResult.Message}");

            return View("Index");
        }

        var coursesVM = new List<CourseViewModel>();

        if (coursesResult.Data != null)
        {
            coursesResult.Data.ForEach(course =>
            {
                var courseVM = new CourseViewModel();
                course.MapTo(courseVM);
                coursesVM.Add(courseVM);
            });
        }

        int pageSize = 6;
        int pageNumber = (page ?? 1);
        ViewBag.OnePageOfAssignemnts = coursesVM;

        return View(coursesVM.ToPagedList(pageNumber, pageSize));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var courseViewModel = new CourseViewModel();

        return View(courseViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CourseViewModel courseViewModel)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var course = new Course()
        {
            Name = courseViewModel.Name
        };

        var createResult =
            await _courseService.CreateCourse(course, currentUserResult.Data, courseViewModel.UploadImage);

        if (!createResult.IsSuccessful)
        {
            _logger.LogError("Failed to create course! Error: {errorMessage}", createResult.Message);
            TempData.TempDataMessage("Error", $"{createResult.Message}");

            return View(courseViewModel);
        }

        await _activityService.AddCreatedCourseActivity(currentUserResult.Data, course);

        await _notificationService.AddCreatedCourseNotification(currentUserResult.Data, course);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var courseResult = await _courseService.GetById(id);

        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                id, courseResult.Message);
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");

            return View("Index");
        }

        var courseVM = new CourseViewModel();
        courseResult.Data.MapTo(courseVM);

        return View(courseVM);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CourseViewModel newCourse)
    {
        var updateNameResult = await _courseService.UpdateName(newCourse.Id, newCourse.Name);

        if (newCourse.UploadImage != null)
        {
            var updateBackgroundResult = await _courseService.UpdateBackground(newCourse.Id, newCourse.UploadImage);

            if (!updateBackgroundResult.IsSuccessful)
            {
                _logger.LogError("Failed to update course by Id {courseId}! Error: {errorMessage}",
                    newCourse.Id, updateBackgroundResult.Message);
                TempData.TempDataMessage("Error", $"{updateBackgroundResult.Message}");

                return View(newCourse);
            }
        }

        if (!updateNameResult.IsSuccessful)
        {
            _logger.LogError("Failed to update course by Id {courseId}! Error: {errorMessage}",
                newCourse.Id, updateNameResult.Message);
            TempData.TempDataMessage("Error", $"{updateNameResult.Message}");

            return View(newCourse);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var courseResult = await _courseService.GetById(id);

        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                id, courseResult.Message);
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");

            return View("Index");
        }

        return View(courseResult.Data);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleteResult = await _courseService.DeleteCourse(id);

        if (!deleteResult.IsSuccessful)
        {
            _logger.LogError("Failed to delete course by Id {courseId}! Error: {errorMessage}",
                id, deleteResult.Message);
            TempData.TempDataMessage("Error", $"{deleteResult.Message}");

            return View("Delete");
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var courseResult = await _courseService.GetById(id);

        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                id, courseResult.Message);
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");

            return View("Index");
        }

        var currentGroups = courseResult.Data.Groups
            .Where(group => group.UserGroups.Any(ug => ug.AppUserId == currentUserResult.Data.Id))
            .ToList();

        var courseViewModel = new CourseViewModel();
        courseResult.Data.MapTo(courseViewModel);

        courseViewModel.CurrentUser = currentUserResult.Data;
        courseViewModel.UserCoursesWithoutAdmins = courseResult.Data.UserCourses
            .Where(ug => ug.AppUser.Role != AppUserRoles.Admin).ToList();

        if (courseViewModel.CurrentUser == null)
        {
            _logger.LogError("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        courseViewModel.CurrentGroups = currentGroups;

        return View(courseViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SelectTeachers(int courseId, int? page, string? searchQuery,
        string? currentQueryFilter)
    {
        if (searchQuery != null)
        {
            page = 1;
        }
        else
        {
            searchQuery = currentQueryFilter;
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

        var userCoursesForCourse = courseResult.Data.UserCourses.Select(uc => uc.AppUserId).ToList();

        var teachers = _userManager.Users
            .Where(u => u.Role == AppUserRoles.Teacher)
            .Select(u => new TeacherViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                ProfileImage = u.ProfileImage,
                IsInvited = userCoursesForCourse.Contains(u.Id),
                CourseId = courseId
            })
            .ToList();

        if (searchQuery != null)
        {
            teachers = teachers.Where(t =>
                t.FirstName.ToLower().Contains(searchQuery.ToLower()) ||
                t.LastName.ToLower().Contains(searchQuery.ToLower())).ToList();
        }

        int pageSize = 9;
        int pageNumber = (page ?? 1);
        ViewBag.SelectTeachersCourseId = courseId;

        return View(teachers.ToPagedList(pageNumber, pageSize));
    }

    [HttpPost]
    public async Task<IActionResult> SendInvitation(string teacherId, int courseId)
    {
        var teacherResult = await _userService.FindByIdAsync(teacherId);
        var courseResult = await _courseService.GetById(courseId);

        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                courseId, courseResult.Message);
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");

            return View("Index");
        }

        if (!teacherResult.IsSuccessful)
        {
            _logger.LogError("Failed to get user by Id {userId}! Error: {errorMessage}",
                teacherId, teacherResult.Message);
            ViewData.ViewDataMessage("Error", $"{teacherResult.Message}");

            return View("Index");
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(teacherResult.Data);
        var callbackUrl = Url.Action(
            "ConfirmTeacherForCourse",
            "Course",
            new { courseId = courseId, code = code },
            protocol: HttpContext.Request.Scheme);

        var sendResult = await _emailService.SendEmailToAppUsers(EmailType.CourseInvitation, teacherResult.Data,
            callbackUrl, course: courseResult.Data);

        if (!sendResult.IsSuccessful)
        {
            _logger.LogError(
                "Failed to send email with invitation to course {courseId} to teacher {teacherId}! Error: {errorMessage}",
                courseResult.Data.Id, teacherResult.Data.Id, sendResult.Message);
            TempData.TempDataMessage("Error", sendResult.Message);

            return View("SelectTeachers");
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ConfirmTeacherForCourse(int courseId, string code)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var currentUser = currentUserResult.Data;
        var courseResult = await _courseService.GetById(courseId);

        if (!courseResult.IsSuccessful)
        {
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                courseId, courseResult.Message);
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");

            return View("Index");
        }

        var courseTeachers = courseResult.Data.UserCourses.Select(c => c.AppUserId).ToList();

        if (courseTeachers.Contains(currentUser.Id))
        {
            TempData.TempDataMessage("Error", "You are already registered for the course");

            return RedirectToAction("Index");
        }

        if (currentUser == null)
        {
            _logger.LogError("Unauthorized user");
            ViewData.ViewDataMessage("Error", "CurrentUser not found");

            return View("Index");
        }

        var result = await _userManager.ConfirmEmailAsync(currentUser, code);

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to confirm email for user {userId} with code {userCode}!",
                currentUser.Id, code);

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error: {errorMessage}", error.Description);
            }

            ViewData.ViewDataMessage("Error", "Confirm email is not successful");

            return View("Index");
        }

        var addTeacherToCourseResult = await _userCourseService.AddUserInCourse(currentUser, courseResult.Data);

        if (!addTeacherToCourseResult.IsSuccessful)
        {
            _logger.LogError("Failed to add teacher {teacherId} to course {courseId}! Error: {errorMessages}",
                currentUser.Id, courseResult.Data.Id, addTeacherToCourseResult.Message);
            ViewData.ViewDataMessage("Error", $"{addTeacherToCourseResult.Message}");

            return View("Index");
        }

        await _activityService.AddJoinedCourseActivity(currentUser, courseResult.Data);

        await _notificationService.AddJoinedCourseNotification(currentUser, courseResult.Data);

        return View();
    }
}