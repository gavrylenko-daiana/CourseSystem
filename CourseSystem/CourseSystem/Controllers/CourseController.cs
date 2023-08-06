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

        if (currentUser == null) return RedirectToAction("Login", "Account");

        var courses = await _courseService.GetByPredicate(course =>
            course.UserCourses.Any(uc => uc.AppUser.Id == currentUser.Id)
        );

        var userCoursesViewModel = new UserCoursesViewModel()
        {
            CurrentUser = currentUser,
            Courses = courses
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
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                TempData.TempDataMessage("Error", "User not found");
                
                return View(courseViewModel);
            }

            var course = new Course()
            {
                Name = courseViewModel.Name
            };

            await _courseService.CreateCourse(course, currentUser);

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ViewData.ViewDataMessage("CreatingError", $"Failed to create course. Error: {e.Message}");

            return View(courseViewModel);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var course = await _courseService.GetById(id);
            
            if (course == null)
            {
                ViewData.ViewDataMessage("Error", "Course not found");

                return View("Error");
            }

            return View(course);
        }
        catch (Exception e)
        {
            ViewData.ViewDataMessage("EditingError", $"Failed to editing course. Error: {e.Message}");
            
            return View("Error");
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(Course newCourse)
    {
        try
        {
            await _courseService.UpdateName(newCourse.Id, newCourse.Name);

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ViewData.ViewDataMessage("EditingError", $"Failed to editing course. Error: {e.Message}");
            
            return View(newCourse);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var course = await _courseService.GetById(id);
            
            if (course == null)
            {
                ViewData.ViewDataMessage("Error", "Course not found");

                return View("Error");
            }
            
            var courseViewModel = new CourseViewModel()
            {
                Name = course.Name
            };

            return View(courseViewModel);
        }
        catch (Exception e)
        {
            ViewData.ViewDataMessage("DeletingError", $"Failed to delete course. Error: {e.Message}");
            
            return View("Error");
        }
    }
    
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var course = await _courseService.GetById(id);
            
            if (course == null)
            {
                ViewData.ViewDataMessage("Error", "Course not found");

                return View("Error");
            }

            await _courseService.DeleteCourse(course.Id);

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ViewData["DeletingError"] = $"Failed to delete course. Error: {e.Message}";
            
            return View("Error");
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null) return RedirectToAction("Login", "Account");
        
        var course = await _courseService.GetById(id);
        
        if (course == null) return NotFound();
        
        var currentGroups = course.Groups
            .Where(group => group.UserGroups.Any(ug => ug.AppUserId == currentUser.Id))
            .ToList();
        
        var courseViewModel = new CourseViewModel();
        course.MapTo(courseViewModel);
        courseViewModel.CurrentUser = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException();
        courseViewModel.CurrentGroups = currentGroups;
        
        TempData["CourseId"] = id;

        return View(courseViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SelectTeachers()
    {
        var courseId = (int)(TempData["CourseId"] ?? throw new InvalidOperationException());
        var course = await _courseService.GetById(courseId);
        
        if (course == null)
        {
            return NotFound();
        }

        var userCoursesForCourse = course.UserCourses.Select(uc => uc.AppUserId).ToList();

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
        var course = await _courseService.GetById(courseId);

        if (teacher == null || course == null)
        {
            return NotFound();
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(teacher);
        var callbackUrl = Url.Action(
           "ConfirmTeacherForCourse",
            "Course",
           new { courseId = courseId, code = code },
           protocol: HttpContext.Request.Scheme);

        var sendResult = await _emailService.SendToTeacherCourseInventation(teacher, course, callbackUrl);
        if (!sendResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", sendResult.Message);
            return View("SelectTeachers");
        }

        TempData["CourseId"] = course.Id;
        
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ConfirmTeacherForCourse(int courseId, string code)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var course = await _courseService.GetById(courseId);
        var courseTecahers = course.UserCourses.Select(c => c.AppUserId).ToList();

        if (courseTecahers.Contains(currentUser.Id))
        {
            TempData.TempDataMessage("Error", "You are already registered for the course");
            return RedirectToAction("Index", "Course");
        }

        if (currentUser == null || course == null)
            return NotFound();

        var result = await _userManager.ConfirmEmailAsync(currentUser, code);

        if (!result.Succeeded)
            return View("Error");

        try
        {
            await _userCourseService.AddTeacherToCourse(course, currentUser);
        }
        catch(Exception ex)
        {
            throw new Exception("Fail to add teacher to course");
        }

        return View();
    }
}