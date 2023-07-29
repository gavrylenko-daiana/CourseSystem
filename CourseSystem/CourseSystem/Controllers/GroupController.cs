using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers;

public class GroupController : Controller
{
    private readonly IGroupService _groupService;
    private readonly ICourseService _courseService;
    private readonly UserManager<AppUser> _userManager;

    public GroupController(IGroupService groupService, ICourseService courseService,
        UserManager<AppUser> userManager)
    {
        _groupService = groupService;
        _courseService = courseService;
        _userManager = userManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null) return RedirectToAction("Login", "Account");

        var groups = await _groupService.GetByPredicate(g =>
            g.UserGroups.Any(ug => ug.AppUserId.Equals(currentUser.Id)));

        var userGroupsViewModel = new UserGroupsViewModel()
        {
            Groups = groups,
            CurrentUser = currentUser
        };
        
        return View(userGroupsViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(GroupViewModel groupViewModel)
    {
        var courseId = (int)(TempData["CourseId"] ?? throw new InvalidOperationException());

        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            TempData.TempDataMessage("Error", "User not found");
                
            return View(groupViewModel);
        }

        var group = new Group();
        groupViewModel.MapTo(group);
        group.CourseId = courseId;

        await _groupService.CreateGroup(group, currentUser);

        return RedirectToAction("Details");
    }

}