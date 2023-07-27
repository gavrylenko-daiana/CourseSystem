using BLL.Interfaces;
using Core.Helpers;
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
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null) return View("Error");

        var courses = await _courseService.GetByPredicate(course =>
            course.UserCourses.Any(uc => uc.AppUser.Id == currentUser.Id)
        );

        var courseViewModels = courses.Select(c =>
        {
            var courseViewModel = new CourseViewModel();
            c.MapTo(courseViewModel);
            return courseViewModel;
        }).ToList();

        return View(courseViewModels);
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
                throw new Exception("User not found");
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
            ViewData["CreatingError"] = $"Failed to create course. Error: {e.Message}";
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
                throw new Exception("Course not found");
            }

            var courseViewModel = new CourseViewModel
            {
                Name = course.Name
            };

            return View(courseViewModel);
        }
        catch (Exception e)
        {
            ViewData["EditingError"] = $"Failed to editing course. Error: {e.Message}";
            return View("Error");
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(CourseViewModel courseViewModel)
    {
        try
        {
            var course = await _courseService.GetById(courseViewModel.Id);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            course.Name = courseViewModel.Name;

            await _courseService.Update(course);

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ViewData["EditingError"] = $"Failed to editing course. Error: {e.Message}";
            return View(courseViewModel);
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
                throw new Exception("Course not found");
            }

            var courseViewModel = new CourseViewModel()
            {
                Name = course.Name
            };

            return View(courseViewModel);
        }
        catch (Exception e)
        {
            ViewData["DeletingError"] = $"Failed to delete course. Error: {e.Message}";
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
                throw new Exception("Course not found");
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
        var course = await _courseService.GetById(id);
        if (course == null)
        {
            return NotFound();
        }

        var courseViewModel = new CourseViewModel();
        course.MapTo(courseViewModel);

        return View(courseViewModel);
    }
}