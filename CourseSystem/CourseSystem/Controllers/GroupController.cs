using BLL.Interfaces;
using Core.Enums;
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
            TempData.TempDataMessage("CreatingError", "User not found");
                
            return View(groupViewModel);
        }

        var group = new Group();
        groupViewModel.MapTo(group);
        group.CourseId = courseId;

        try
        {
            await _groupService.CreateGroup(group, currentUser);
        }
        catch (Exception ex)
        {
            TempData["CourseId"] = courseId; 
            
            TempData.TempDataMessage("CreatingError", "Your end day must be more than start day");
                
            return View(groupViewModel);
        }

        return RedirectToAction("Details", "Group", new { id = group.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var group = await _groupService.GetById(id);
        
        if (group == null)
        {
            return NotFound();
        }
        
        TempData["GroupId"] = id;

        var userGroupVM = new UserGroupViewModel()
        {
            Group = group,
            CurrentUser = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException()
        };

        return View(userGroupVM);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var group = await _groupService.GetById(id);
        
        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Group newGroup)
    {
        await _groupService.UpdateGroup(newGroup.Id, newGroup.Name, newGroup.StartDate, newGroup.EndDate);
        
        return RedirectToAction("Details", new { id = newGroup.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var group = await _groupService.GetById(id);
        
        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var group = await _groupService.GetById(id);
        
        if (group == null)
        {
            return NotFound();
        }

        await _groupService.DeleteGroup(id);
        
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> SelectStudent()
    {
        var groupId = (int)(TempData["GroupId"] ?? throw new InvalidOperationException());
        var group = await _groupService.GetById(groupId);

        var students = await _userManager.GetUsersInRoleAsync("Student");
        
        var studentsInGroupIds = group.UserGroups.Select(ug => ug.AppUserId);
        
        var availableStudents = students.Where(s => !studentsInGroupIds.Contains(s.Id))
            .Select(u => new StudentSelectionViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsSelected = false
            })
            .ToList();
        
        ViewBag.GroupId = groupId;

        return View(availableStudents);
    }
    
    [HttpPost]
    public async Task<IActionResult> ConfirmSelection(int groupId, List<StudentSelectionViewModel> students)
    {
        var selectedStudents = students.Where(s => s.IsSelected).ToList();
        
        if (selectedStudents.Count > 20)
        {
            //send approval to admin
        }

        return RedirectToAction("Index");
    }

}