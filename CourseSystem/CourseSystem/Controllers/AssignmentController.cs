using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UI.ViewModels;
using UI.ViewModels.AssignmentViewModels;
using X.PagedList;
using static Dropbox.Api.Files.ListRevisionsMode;
using static Dropbox.Api.Team.GroupSelector;


namespace UI.Controllers;

[Authorize]
public class AssignmentController : Controller
{
    private readonly IAssignmentService _assignmentService;
    private readonly IUserService _userService; 
    private readonly IUserAssignmentService _userAssignmentService;
    private readonly IActivityService _activityService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AssignmentController> _logger;
    private readonly IAssignmentFileService _assignmentFileService;

    public AssignmentController(IAssignmentService assignmentService, IUserService userService,
        IUserAssignmentService userAssignmentService, IActivityService activityService,
            INotificationService notificationService, ILogger<AssignmentController> logger, IAssignmentFileService assignmentFileService)
    {
        _assignmentService = assignmentService;
        _userService = userService;
        _userAssignmentService = userAssignmentService;
        _activityService = activityService;
        _notificationService = notificationService;
        _logger = logger;
        _assignmentFileService = assignmentFileService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int groupId, string currentQueryFilter, string currentAccessFilter, SortingParam sortOrder, string searchQuery, string assignmentAccessFilter, int? page) 
    {
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParam = sortOrder == SortingParam.NameDesc ? SortingParam.Name : SortingParam.NameDesc;
        ViewBag.StartDateParam = sortOrder == SortingParam.StartDateDesc ? SortingParam.StartDate : SortingParam.StartDateDesc;
        ViewBag.EndDateParam = sortOrder == SortingParam.EndDateDesc ? SortingParam.EndDate : SortingParam.EndDateDesc;

        if(searchQuery != null || assignmentAccessFilter != null)
        {
            page = 1;
        }
        else
        {
            searchQuery = currentQueryFilter;
            assignmentAccessFilter = currentAccessFilter;
        }

        ViewBag.CurrentQueryFilter = searchQuery;
        ViewBag.CurrentAccessFilter = assignmentAccessFilter;

        var groupAssignmentsResult = await _assignmentService.GetGroupAssignments(groupId, sortOrder, assignmentAccessFilter, searchQuery);

        if (!groupAssignmentsResult.IsSuccessful)
        {
            _logger.LogError("Failed to get assignments for group {groupId}! Error: {errorMessage}",
                groupId, groupAssignmentsResult.Message);

            TempData.TempDataMessage("Error", groupAssignmentsResult.Message);

            return RedirectToAction("Index", "Home");
        }

        var assignmentsVM = new List<AssignmentViewModel>();

        if (groupAssignmentsResult.Data != null)
        {
            groupAssignmentsResult.Data.ForEach(assignment =>
            {
                var assignmentVM = new AssignmentViewModel();
                assignment.MapTo(assignmentVM);
                
                assignmentVM.UserAssignment = assignment.UserAssignments.FirstOrDefault(ua => ua.AssignmentId == assignment.Id);
                assignmentsVM.Add(assignmentVM);
            });
        }

        ViewBag.GroupId = groupId;
        int pageSize = 6;
        int pageNumber = (page ?? 1);
        ViewBag.OnePageOfAssignemnts = assignmentsVM;

        //var currentUserResult = await _userService.GetCurrentUser(User);
        //await _assignmentService.GetAllUserAssignemnts(currentUserResult.Data);
        return View(assignmentsVM.ToPagedList(pageNumber, pageSize));
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create(int groupId)
    {
        var assignmentVM = new CreateAssignmentViewModel() { GroupId = groupId };

        return View(assignmentVM);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAssignmentViewModel assignmentVM)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        if (assignmentVM == null)
        {
            _logger.LogError("Failed to create assignment - assignment view model was null!");

            return View("Error");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogError("Failed to create assignment - invalid input data!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("Error: {errorMessage}", error.ErrorMessage);
            }

            TempData.TempDataMessage("Error", "Invalid input data");

            return View(assignmentVM);
        }

        var checkTimeResult = _assignmentService.ValidateTimeInput(assignmentVM.StartDate, assignmentVM.EndDate);
        
        if (!checkTimeResult.IsSuccessful)
        {
            _logger.LogError("Failed to create assignment! Error: {errorMessage}", checkTimeResult.Message);

            TempData.TempDataMessage("Error", checkTimeResult.Message);

            return View(assignmentVM);
        }

        var assignment = new Assignment();
        LibraryForMapping.MapTo(assignmentVM, assignment);

        var createResult = await _assignmentService.CreateAssignment(assignment);

        if (!createResult.IsSuccessful)
        {
            _logger.LogError("Failed to create assignment! Error: {errorMessage}", createResult.Message);

            TempData.TempDataMessage("Error", createResult.Message);

            return View(assignmentVM);
        }
        
        if (assignmentVM.AttachedFiles != null)
        {
            var assignmentFile = await _assignmentFileService.AddAssignmentFile(DropboxFolders.TeacherAssignmentFiles, assignmentVM.AttachedFiles, assignment.Id);

            if (!assignmentFile.IsSuccessful)
            {
                TempData.TempDataMessage("Error", $"Message - {assignmentFile.Message}");
                
                return View(assignmentVM);
            }
        }

        await _activityService.AddCreatedAssignmentActivity(currentUserResult.Data, assignment);

        await _notificationService.AddCreatedAssignmentNotification(currentUserResult.Data, assignment);

        return RedirectToAction("Index", "Assignment", new { groupId = assignmentVM.GroupId });
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int assignmentId)
    {
        var assignmentResult = await _assignmentService.GetById(assignmentId);

        if (!assignmentResult.IsSuccessful)
        {
            _logger.LogError("Failed to delete assignment by Id {assignmentId}! Error: {errorMessage}",
                assignmentId, assignmentResult.Message);

            TempData.TempDataMessage("Error", $"{assignmentResult.Data}");

            return RedirectToAction("Index", "Group");
        }

        return View(assignmentResult.Data);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        var deleteResult = await _assignmentService.DeleteAssignment(id);

        if (!deleteResult.IsSuccessful)
        {
            _logger.LogError("Failed to delete assignment by Id {assignmentId}! Error: {errorMessage}",
                id, deleteResult.Message);
            TempData.TempDataMessage("Error", deleteResult.Message);

            return RedirectToAction("Delete", "Assignment", new { assignmentId = id });
        }

        return RedirectToAction("Index", "Group");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int assignmentId)
    {
        var assignmentResult = await _assignmentService.GetById(assignmentId);

        if (!assignmentResult.IsSuccessful)
        {
            _logger.LogError("Failed to get details for assignment by Id {assignmentId}! Error: {errorMessage}",
                assignmentId, assignmentResult.Message);
            TempData.TempDataMessage("Error", $"{assignmentResult.Data}");

            return RedirectToAction("Index", "Group");
        }

        var assignentDetailsVM = new DetailsAssignmentViewModel();
        assignmentResult.Data.MapTo(assignentDetailsVM);

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{currentUserResult.Data}");

            return RedirectToAction("Index", "Assignment", new { groupId = assignmentResult.Data.GroupId});
        }

        var userAssignemntResult = await _userAssignmentService.GetUserAssignemnt(assignmentResult.Data, currentUserResult.Data);
        
        if (!userAssignemntResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{userAssignemntResult.Data}");

            return RedirectToAction("Index", "Assignment", new { groupId = assignmentResult.Data.GroupId });
        }
        
        assignentDetailsVM.UserAssignment = userAssignemntResult.Data;

        if (userAssignemntResult.Data?.AssignmentAnswers == null)
        {
            assignentDetailsVM.AssignmentAnswers = new List<AssignmentAnswer>();
        }
        else
        {
            assignentDetailsVM.AssignmentAnswers = userAssignemntResult.Data.AssignmentAnswers;
        }

        assignentDetailsVM.AssignmentFiles = assignmentResult.Data.AssignmentFiles;

        return View(assignentDetailsVM);
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Edit(int id)
    {
        var assignmentResult = await _assignmentService.GetById(id);

        if (!assignmentResult.IsSuccessful)
        {
            _logger.LogError("Failed to edit assignment by Id {assignmentId}! Error: {errorMessage}",
                id, assignmentResult.Message);
            TempData.TempDataMessage("Error", $"{assignmentResult.Data}");

            return RedirectToAction("Details", "Assignment", new { assignmentId = id });
        }

        var assigmentVM = new EditAssignmentViewModel();
        assignmentResult.Data.MapTo(assigmentVM);
        
        return View(assigmentVM);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditAssignmentViewModel editAssignmentVM)
    {
        if (editAssignmentVM == null)
        {
            _logger.LogError("Failed to edit assignment - assignment view model wasn't received!");

            return View("Error");
        }

        var checkTimeResult =
            _assignmentService.ValidateTimeInput(editAssignmentVM.StartDate, editAssignmentVM.EndDate);
        
        if (!checkTimeResult.IsSuccessful)
        {
            _logger.LogError("Failed to edir assignment by Id {assignmentId}! Error: {errorMessage}", editAssignmentVM.Id, checkTimeResult.Message);

            TempData.TempDataMessage("Error", checkTimeResult.Message);

            return View(editAssignmentVM);
        }

        var assignment = new Assignment();
        editAssignmentVM.MapTo(assignment);

        if (editAssignmentVM.NewAddedFiles != null)
        {
            var assignmentFile = await _assignmentFileService.AddAssignmentFile(DropboxFolders.TeacherAssignmentFiles, editAssignmentVM.NewAddedFiles, assignment.Id);

            if (!assignmentFile.IsSuccessful)
            {
                TempData.TempDataMessage("Error", $"Message - {assignmentFile.Message}");
                
                return View(editAssignmentVM);
            }
        }
        
        var updateAssignmentResult = await _assignmentService.UpdateAssignment(assignment);

        if (!updateAssignmentResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", updateAssignmentResult.Message);

            return View(editAssignmentVM);
        }

        return RedirectToAction("Details", "Assignment", new { assignmentId = editAssignmentVM.Id });
    }
}