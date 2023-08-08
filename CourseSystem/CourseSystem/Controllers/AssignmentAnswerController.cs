using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UI.ViewModels;

namespace UI.Controllers;

[Authorize]
public class AssignmentAnswerController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IAssignmentService _assignmentService;
    private readonly IAssignmentAnswerService _assignmentAnswerService;
    private readonly IUserAssignmentService _userAssignmentService;

    public AssignmentAnswerController(UserManager<AppUser> userManager,
        IAssignmentService assignmentService,
        IAssignmentAnswerService assignmentAnswerService,
        IUserAssignmentService userAssignmentService)
    {
        _userManager = userManager;
        _assignmentService = assignmentService;
        _assignmentAnswerService = assignmentAnswerService;
        _userAssignmentService = userAssignmentService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreateAnswer(int id)
    {
        var assignmnetAnsweVM = new AssignmentAnsweViewModel()
        {
            AssignmentId = id
        };
        
        if (assignmnetAnsweVM == null) throw new ArgumentNullException(nameof(assignmnetAnsweVM));

        return View(assignmnetAnsweVM);
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Create(AssignmentAnsweViewModel assignmentAnswerVM)
    {
        if (!ModelState.IsValid)
        {
            return View(assignmentAnswerVM);
        }

        var assignmnetAnswer = new AssignmentAnswer();
        assignmentAnswerVM.MapTo<AssignmentAnsweViewModel, AssignmentAnswer>(assignmnetAnswer);

        if (!assignmentAnswerVM.AssignmentAnswerFiles.IsNullOrEmpty())
        {
            //logic for files
            //set the name of file to model
        }

        assignmnetAnswer.Name = "Some file name";
        assignmnetAnswer.Text = assignmentAnswerVM.AnswerDescription; //markdown
        assignmnetAnswer.Url = "Some URL";

        var assignmnet = await _assignmentService.GetById(assignmentAnswerVM.AssignmentId);
        var currentUser = await _userManager.GetUserAsync(User);

        var answerResult =
            await _assignmentAnswerService.CreateAssignmentAnswer(assignmnetAnswer, assignmnet, currentUser);

        if (!answerResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", "Fail to save assignmnet answer");
            return RedirectToAction("CreateAnswer", "AssignmentAnswer", new { assignmentAnswerVM.AssignmentId });
        }

        return RedirectToAction("Details", "Assignment", new { id = assignmnet.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Delete(int assignmentAnswerId)
    {
        var assignmentAnswer = await _assignmentAnswerService.GetById(assignmentAnswerId);
        var asignmentId = assignmentAnswer.UserAssignment.AssignmentId;

        if (assignmentAnswer == null)
        {
            return NotFound();
        }

        var deleteResult = await _assignmentAnswerService.DeleteAssignmentAnswer(assignmentAnswer);

        if (!deleteResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", deleteResult.Message);
        }

        return RedirectToAction("Details", "Assignment", new { id = asignmentId });
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> SeeStudentAnswers(int assignmentId)
    {
        var assignment = await _assignmentService.GetById(assignmentId);

        if (assignment == null)
        {
            return NotFound();
        }

        var userAssignmentVMs = new List<UserAssignmentViewModel>();

        foreach (var userAssignment in assignment.UserAssignments)
        {
            var userAssignmentVM = new UserAssignmentViewModel();
            userAssignment.MapTo<UserAssignments, UserAssignmentViewModel>(userAssignmentVM);
            userAssignmentVM.Id = userAssignment.Id;
            userAssignmentVMs.Add(userAssignmentVM);
        }

        return View(userAssignmentVMs);
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CheckAnswer(int assignmentId, string studentId)
    {
        var student = await _userManager.FindByIdAsync(studentId);

        if (student == null)
        {
            return NotFound();
        }

        var assignment = await _assignmentService.GetById(assignmentId);
        var userAssignment = assignment.UserAssignments.FirstOrDefault(a => a.AppUserId == student.Id);

        if (userAssignment == null)
        {
            return NotFound();
        }

        var checkAnswerVM = new CheckAnswerViewModel();
        userAssignment.MapTo<UserAssignments, CheckAnswerViewModel>(checkAnswerVM);

        TempData["AssignmentId"] = assignmentId;
        TempData["StudentId"] = studentId;

        return View(checkAnswerVM);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ChangeGrade(int grade)
    {
        var assignmentId = (int)TempData["AssignmentId"];
        var studentId = TempData["StudentId"].ToString();

        if (grade < 0 || grade > 100)
        {
            TempData.TempDataMessage("Error", "Grade can't be more than 100 or less than 0");
            return RedirectToAction("CheckAnswer", "AssignmentAnswer",
                new { assignmentId = assignmentId, studentId = studentId });
        }

        var assignment = await _assignmentService.GetById(assignmentId);
        var userAssignment = assignment.UserAssignments.FirstOrDefault(a => a.AppUserId == studentId);

        if (userAssignment == null)
        {
            return NotFound();
        }

        var updateResult = await _userAssignmentService.ChangeUserAssignmentGrade(userAssignment, grade);

        if (!updateResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", updateResult.Message);

            return RedirectToAction("CheckAnswer", "AssignmentAnswer",
                new { assignmentId = userAssignment.AssignmentId, studentId = userAssignment.AppUserId });
        }

        return RedirectToAction("SeeStudentAnswers", "AssignmentAnswer",
            new { assignmentId = userAssignment.AssignmentId });
    }
}