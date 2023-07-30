using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using UI.ViewModels;

namespace UI.Controllers;

public class GroupController : Controller
{
    private readonly IGroupService _groupService;
    private readonly ICourseService _courseService;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;

    public GroupController(IGroupService groupService, ICourseService courseService,
        UserManager<AppUser> userManager,
        IEmailService emailService)
    {
        _groupService = groupService;
        _courseService = courseService;
        _emailService = emailService;
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

        var group = new Core.Models.Group();
        groupViewModel.MapTo(group);
        group.CourseId = courseId;

        await _groupService.CreateGroup(group, currentUser);

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

        return View(group);
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
    public async Task<IActionResult> Edit(Core.Models.Group newGroup)
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
    public async Task<IActionResult> SelectStudent(int id, bool approved = false)
    {
        var groupId = (int)(TempData["GroupId"] ?? id);
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
        ViewBag.Approved = approved;

        return View(availableStudents);
    }
    
    [HttpPost]
    public async Task<IActionResult> ConfirmSelection(int groupId, List<StudentSelectionViewModel> students)
    {
        var selectedStudents = students.Where(s => s.IsSelected).ToList();
        
        if (selectedStudents.Count > 20)
        {
            TempData.TempDataMessage("Error", "Group cannot be more than 20 students without admin confirmation");

            return View("GetApprove", groupId);
        }
        else
        {
            //logic for creation
        }

        return RedirectToAction("Index");
    }
    [HttpPost]
    public async Task<IActionResult> ApprovedSelection(int groupId, List<StudentSelectionViewModel> students)
    {
        var selectedStudents = students.Where(s => s.IsSelected).ToList();

        //cretion logic

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("GetApprove/{id}")]
    public async Task<IActionResult> GetApprove(int id)
    {
        //send email to admin for getting approve
        var group = await _groupService.GetById(id);
        if (group == null)
            return View("Error");

        var currentTeacher = await _userManager.GetUserAsync(User);

        var callbackUrl = Url.Action( 
            "AdminApprove",
            "Group",
            new { groupId = id, teacherId = currentTeacher.Id },
            protocol: HttpContext.Request.Scheme);

        var sendEmailResult = await _emailService.SendToAdminConfirmationForGroups(group, callbackUrl);

        if (!sendEmailResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", sendEmailResult.Message);
            return RedirectToAction("Index", "Home");
        }

        //TempData.TempDataMessage("Error", sendEmailResult.Message);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> AdminApprove(int groupId, string teacherId)
    {
        var group = await _groupService.GetById(groupId);
        var teacher = await _userManager.FindByIdAsync(teacherId);
        if (group == null || teacher == null)
            return View("Error");

        var callbackUrl = Url.Action( 
           "ApprovedGroup",
           "Group",
           new { groupId = groupId},
           protocol: HttpContext.Request.Scheme);

        await _emailService.SendEmailToTeacherAboutApprovedGroup(teacher, group, callbackUrl);

        return View();
    }


    [HttpGet]
    public async Task<IActionResult> ApprovedGroup(int groupId)
    {
        var group = await _groupService.GetById(groupId);

        if (group == null)
            return View("Error");

        return RedirectToAction("SelectStudent", "Group", new { id = groupId, approved = true });
    }

}