using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using UI.ViewModels;

namespace UI.Controllers;

[Authorize]
public class CourseController : Controller
{
    private readonly ICourseService _courseService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IUserCourseService _userCourseService;

    public CourseController(ICourseService courseService,
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IUserCourseService userCourseService)
    {
        _courseService = courseService;
        _userManager = userManager;
        _emailService = emailService;
        _userCourseService = userCourseService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var coursesResult = await _courseService.GetByPredicate(course =>
            course.UserCourses.Any(uc => uc.AppUser.Id == currentUser.Id)
        );

        if (!coursesResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{coursesResult.Message}");
            return View("Index");
        }
        
        var userCoursesViewModel = new UserCoursesViewModel()
        {
            CurrentUser = currentUser,
            Courses = coursesResult.Data
        };

        return View(userCoursesViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CourseViewModel courseViewModel)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var course = new Course()
        {
            Name = courseViewModel.Name
        };
        
        var createResult = await _courseService.CreateCourse(course, currentUser);
            
        if (!createResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{createResult.Message}");
            return View(courseViewModel);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var courseResult = await _courseService.GetById(id);

        if (!courseResult.IsSuccessful)
        {
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");
            return View("Index");
        }

        return View(courseResult.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Course newCourse)
    {
        var updateResult = await _courseService.UpdateName(newCourse.Id, newCourse.Name);

        if (!updateResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{updateResult.Message}");
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
            TempData.TempDataMessage("Error", $"{deleteResult.Message}");
            return View("Delete");
        }
            
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var courseResult = await _courseService.GetById(id);
        
        if (!courseResult.IsSuccessful)
        {
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");
            return View("Index");
        }

        var currentGroups = courseResult.Data.Groups
            .Where(group => group.UserGroups.Any(ug => ug.AppUserId == currentUser.Id))
            .ToList();

        var courseViewModel = new CourseViewModel();
        courseResult.Data.MapTo(courseViewModel);
        courseViewModel.CurrentUser = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException();
        courseViewModel.CurrentGroups = currentGroups;

        TempData["CourseId"] = id;

        return View(courseViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SelectTeachers()
    {
        var courseId = (int)(TempData["CourseId"] ?? throw new InvalidOperationException());
        var courseResult = await _courseService.GetById(courseId);

        if (!courseResult.IsSuccessful)
        {
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
                IsInvited = userCoursesForCourse.Contains(u.Id)
            })
            .ToList();

        return View(teachers);
    }

    [HttpPost]
    public async Task<IActionResult> SendInvitation(string teacherId, int courseId)
    {
        var teacher = await _userManager.FindByIdAsync(teacherId);
        var courseResult = await _courseService.GetById(courseId);

        if (!courseResult.IsSuccessful)
        {
            ViewData.ViewDataMessage("Error", $"{courseResult.Message}");
            return View("Index");
        }
        
        if (teacher == null)
        {
            ViewData.ViewDataMessage("Error", "Teacher not found");
            return View("Index");
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(teacher);
        var callbackUrl = Url.Action(
            "ConfirmTeacherForCourse",
            "Course",
            new { courseId = courseId, code = code },
            protocol: HttpContext.Request.Scheme);

        var sendResult = await _emailService.SendToTeacherCourseInvitation(teacher, courseResult.Data, callbackUrl);

        if (!sendResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", sendResult.Message);
            return View("SelectTeachers");
        }

        TempData["CourseId"] = courseResult.Data.Id;

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ConfirmTeacherForCourse(int courseId, string code)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }         

        var courseResult = await _courseService.GetById(courseId);
        
        if (!courseResult.IsSuccessful)
        {
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
            ViewData.ViewDataMessage("Error", "CurrentUser not found");
            return View("Index");
        }

        var result = await _userManager.ConfirmEmailAsync(currentUser, code);

        if (!result.Succeeded)
        {
            ViewData.ViewDataMessage("Error", "Confirm email is not successful");
            return View("Index");
        }

        var addTeacherToCourseResult = await _userCourseService.AddTeacherToCourse(courseResult.Data, currentUser);
        
        if (!addTeacherToCourseResult.IsSuccessful)
        {
            ViewData.ViewDataMessage("Error", $"{addTeacherToCourseResult.Message}");
            return View("Index");
        }

        await _userCourseService.AddTeacherToCourse(courseResult.Data, currentUser);

        return View();
    }
}