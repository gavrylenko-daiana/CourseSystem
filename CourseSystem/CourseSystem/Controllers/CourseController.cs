using BLL.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers;

public class CourseController : Controller
{
    private readonly ICourseService _courseService;
    private readonly UserManager<AppUser> _userManager;

    public CourseController(ICourseService courseService, UserManager<AppUser> userManager)
    {
        _courseService = courseService;
        _userManager = userManager;
    }
    
    public async Task<ViewResult> Index()
    {
        // all courses in db (change on user.courses)
        var courses = await _courseService.GetAll();
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null) return View("Error");
        
        var courses = await _courseService.GetByPredicate(course => 
            course.UserCourses.Any(uc => uc.AppUser.Id == currentUser.Id)
        );
        
        var courseViewModels = courses.Select(c => new CourseViewModel
        {
            Name = c.Name
        }).ToList();

        return View(courseViewModels);
    }

    public ActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<ActionResult> Create(CourseViewModel courseViewModel)
    {
        try
        {
            var course = new Course
            {
                Name = courseViewModel.Name
            };

            await _courseService.CreateCourse(course);

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ViewData["CreatingError"] = $"Failed to create course. Error: {e.Message}";
            return View(courseViewModel);
        }
    }
    
}