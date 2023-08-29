using BLL.Interfaces;
using BLL.Services;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.IdentityModel.Tokens;
using UI.ViewModels;
using UI.ViewModels.AssignmentViewModels;
using X.PagedList;
using LinkGenerator = Core.Helpers.LinkGenerator;

namespace UI.Controllers;

[Authorize]
public class AssignmentAnswerController : Controller
{
    private readonly IAssignmentService _assignmentService;
    private readonly IAssignmentAnswerService _assignmentAnswerService;
    private readonly IUserAssignmentService _userAssignmentService;
    private readonly IUserService _userService;
    private readonly IActivityService _activityService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AssignmentAnswerController> _logger;
    private readonly IUrlHelperFactory _urlHelperFactory;

    public AssignmentAnswerController(IAssignmentService assignmentService,
        IAssignmentAnswerService assignmentAnswerService,
        IUserAssignmentService userAssignmentService,
        IUserService userService,
        IActivityService activityService,
        INotificationService notificationService,
        ILogger<AssignmentAnswerController> logger,
        IUrlHelperFactory urlHelperFactory)
    {
        _assignmentService = assignmentService;
        _assignmentAnswerService = assignmentAnswerService;
        _userAssignmentService = userAssignmentService;
        _userService = userService;
        _activityService = activityService;
        _notificationService = notificationService;
        _logger = logger;
        _urlHelperFactory = urlHelperFactory;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index(int fileId)
    {
        var assignmentAnswer = await _assignmentAnswerService.GetById(fileId);

        if (!assignmentAnswer.IsSuccessful)
        {
            TempData.TempDataMessage("Error", assignmentAnswer.Message);

            return RedirectToAction("Index", "Group");
        }
        
        return View(assignmentAnswer.Data);
    }


    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Create(int id)
    {
        var assignmnetAnsweVM = new AssignmentAnsweViewModel()
        {
            AssignmentId = id
        };

        return View(assignmnetAnsweVM);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AssignmentAnsweViewModel assignmentAnswerVM)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Failed to create assignment answer!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("Error: {errorMessage}", error.ErrorMessage);
            }

            TempData.TempDataMessage("Error", "Fail to create assignment answer");

            return View(assignmentAnswerVM);
        }

        var assignmentAnswer = new AssignmentAnswer();
        assignmentAnswerVM.MapTo(assignmentAnswer);

        var assignmentResult = await _assignmentService.GetById(assignmentAnswerVM.AssignmentId);

        if (!assignmentResult.IsSuccessful)
        {
            _logger.LogError("Failed to get assignment by Id {assignmentId}! Error: {errorMessage}",
                assignmentAnswerVM.AssignmentId, assignmentResult.Message);
            TempData.TempDataMessage("Error", assignmentResult.Message);

            return View(assignmentAnswerVM);
        }

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }
        
        assignmentAnswer.Text = assignmentAnswerVM.AnswerDescription; //markdown
        var answerResult = await _assignmentAnswerService.CreateAssignmentAnswer(assignmentAnswer, assignmentResult.Data, currentUserResult.Data, assignmentAnswerVM.AssignmentAnswerFiles, DropboxFolders.StudentAssignmentFiles);
        
        if (!answerResult.IsSuccessful)
        {
            _logger.LogError("Failed to save assignment answer for user {userId}! Errors: {errorMessage}",
                currentUserResult.Data.Id, answerResult.Message);

            TempData.TempDataMessage("Error", "Fail to save assignment answer");

            return RedirectToAction("Create", "AssignmentAnswer", new { assignmentAnswerVM.AssignmentId });
        }

        await _activityService.AddSubmittedAssignmentAnswerActivity(currentUserResult.Data, assignmentResult.Data);

        await _notificationService.AddSubmittedAssignmentForStudentNotification(assignmentAnswer.UserAssignment,
            LinkGenerator.GenerateAssignmentAnswerLink(_urlHelperFactory,this, assignmentAnswer));

        var teachers = assignmentAnswer.UserAssignment.Assignment.UserAssignments
            .Select(ua => ua.AppUser).Where(user => user.Role == Core.Enums.AppUserRoles.Teacher).ToList();

        foreach (var teacher in teachers)
        {
            await _notificationService.AddSubmittedAssignmentForTeacherNotification(teacher, assignmentAnswer.UserAssignment,
                LinkGenerator.GenerateAssignmentAnswerLink(_urlHelperFactory,this, assignmentAnswer));
        }

        return RedirectToAction("Details", "Assignment", new { assignmentId = assignmentResult.Data.Id });
    }


    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Delete(int assignmentAnswerId)
    {
        var assignmentAnswerResult = await _assignmentAnswerService.GetById(assignmentAnswerId);
        var assignmentId = assignmentAnswerResult.Data.UserAssignment.AssignmentId;

        if (!assignmentAnswerResult.IsSuccessful)
        {
            _logger.LogError("Failed to get assignment answer by Id {assignmentAnswerId}! Error: {errorMessage}",
                assignmentAnswerId, assignmentAnswerResult.Message);
            TempData.TempDataMessage("Error", $"{assignmentAnswerResult.Data}");

            return RedirectToAction("Index", "Group");
        }

        var deleteResult = await _assignmentAnswerService.DeleteAssignmentAnswer(assignmentAnswerResult.Data);

        if (!deleteResult.IsSuccessful)
        {
            _logger.LogError("Failed to delete assignment answer by Id {assignmentAnswerId}! Error: {errorMessage}",
                assignmentAnswerResult.Data.Id, deleteResult.Message);
            TempData.TempDataMessage("Error", deleteResult.Message);
        }

        return RedirectToAction("Details", "Assignment", new { assignmentId = assignmentId });
    }


    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> SeeStudentAnswers(int assignmentId, string isMarked, string currentFilter,
        int? page)
    {
        ViewBag.AssignmentId = assignmentId;

        if (isMarked != null)
        {
            page = 1;
        }
        else
        {
            isMarked = currentFilter;
        }

        ViewBag.CurrentFilter = isMarked;

        var userAssignmentsResult = await _userAssignmentService.GetAllUserAssignments(assignmentId, isMarked);

        if (!userAssignmentsResult.IsSuccessful)
        {
            _logger.LogError("Failed to get user assignments by Id {assignmentId}! Error: {errorMessage}",
                assignmentId, userAssignmentsResult.Message);
            TempData.TempDataMessage("Error", $"{userAssignmentsResult.Data}");

            return RedirectToAction("Index", "Group");
        }

        var userAssignmentVMs = new List<UserAssignmentViewModel>();

        foreach (var userAssignment in userAssignmentsResult.Data)
        {
            var userAssignmentVM = new UserAssignmentViewModel();
            userAssignment.MapTo(userAssignmentVM);

            userAssignmentVM.Id = userAssignment.Id;
            userAssignmentVMs.Add(userAssignmentVM);
        }

        int pageSize = 4;
        int pageNumber = (page ?? 1);

        return View(userAssignmentVMs.ToPagedList(pageNumber, pageSize));
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CheckAnswer(int assignmentId, string studentId)
    {
        var studentResult = await _userService.FindByIdAsync(studentId);

        if (!studentResult.IsSuccessful)
        {
            _logger.LogError("Failed to get user by Id {userId}! Error: {errorMessage}",
                studentId, studentResult.Message);

            return NotFound();
        }

        var assignmentResult = await _assignmentService.GetById(assignmentId);

        if (!assignmentResult.IsSuccessful)
        {
            _logger.LogError("Failed to get assignment by Id {assignmentId}! Error: {errorMessage}",
                assignmentId, assignmentResult.Message);
            TempData.TempDataMessage("Error", $"{assignmentResult.Data}");

            return RedirectToAction("Index", "Group");
        }

        var userAssignment =
            assignmentResult.Data.UserAssignments.FirstOrDefault(a => a.AppUserId == studentResult.Data.Id);

        if (userAssignment == null)
        {
            _logger.LogError("Failed to get assignment answer for assignment {assignmentId} by student {studentId}!",
                assignmentResult.Data.Id, studentResult.Data.Id);

            return NotFound();
        }

        var checkAnswerVM = new CheckAnswerViewModel();
        userAssignment.MapTo(checkAnswerVM);

        TempData["AssignmentId"] = assignmentId;
        TempData["StudentId"] = studentId;

        return View(checkAnswerVM);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ChangeGrade(int grade)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var assignmentId = (int)TempData["AssignmentId"];
        var studentId = TempData["StudentId"].ToString();

        if (grade < 0 || grade > 100)
        {
            _logger.LogInformation(
                "Teacher tried to grade assignment {assignmentId} for student {studentId} with unacceptable grade {grade}",
                assignmentId, studentId, grade);
            TempData.TempDataMessage("Error", "Grade can't be more than 100 or less than 0");

            return RedirectToAction("CheckAnswer", "AssignmentAnswer",
                new { assignmentId = assignmentId, studentId = studentId });
        }

        var assignmentResult = await _assignmentService.GetById(assignmentId);

        if (!assignmentResult.IsSuccessful)
        {
            _logger.LogError("Failed to get assignment by Id {assignmentId}! Error: {errorMessage}",
                assignmentId, assignmentResult.Message);
            TempData.TempDataMessage("Error", $"{assignmentResult.Data}");

            return RedirectToAction("Index", "Group");
        }

        var userAssignment = assignmentResult.Data.UserAssignments.FirstOrDefault(a => a.AppUserId == studentId);

        if (userAssignment == null)
        {
            _logger.LogError("Failed to get assignment {assignmentId} for user {userId}!",
                assignmentResult.Data.Id, studentId);

            return NotFound();
        }

        var updateResult = await _userAssignmentService.ChangeUserAssignmentGrade(userAssignment, grade);

        if (!updateResult.IsSuccessful)
        {
            _logger.LogError(
                "Failed to update grade to {grade} for userAssignment by Id {userAssignmentId}! Error: {errorMessage}",
                grade, userAssignment.Id, updateResult.Message);
            TempData.TempDataMessage("Error", updateResult.Message);

            return RedirectToAction("CheckAnswer", "AssignmentAnswer",
                new { assignmentId = userAssignment.AssignmentId, studentId = userAssignment.AppUserId });
        }

        await _activityService.AddMarkedAssignmentActivity(currentUserResult.Data, userAssignment);

        await _notificationService.AddMarkedAssignmentForStudentNotification(userAssignment,
            LinkGenerator.GenerateAssignmentLink(_urlHelperFactory,this, assignmentResult.Data));
        
        await _notificationService.AddMarkedAssignmentForTeacherNotification(currentUserResult.Data, userAssignment,
            LinkGenerator.GenerateAssignmentLink(_urlHelperFactory,this, assignmentResult.Data));

        return RedirectToAction("SeeStudentAnswers", "AssignmentAnswer",
            new { assignmentId = userAssignment.AssignmentId });
    }
}