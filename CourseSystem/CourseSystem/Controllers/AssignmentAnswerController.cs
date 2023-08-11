using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UI.ViewModels;
using UI.ViewModels.AssignmentViewModels;

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
            TempData.TempDataMessage("Error", "Fail to create assignment answer");
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

        var assignmnetResult = await _assignmentService.GetById(assignmentAnswerVM.AssignmentId);


        if (!assignmnetResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", assignmnetResult.Message);
            return View(assignmentAnswerVM);
        }

        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var answerResult =
            await _assignmentAnswerService.CreateAssignmentAnswer(assignmnetAnswer, assignmnetResult.Data, currentUser);

        if (!answerResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", "Fail to save assignmnet answer");
            return RedirectToAction("Create", "AssignmentAnswer", new { assignmentAnswerVM.AssignmentId });
        }

        return RedirectToAction("Details", "Assignment", new { id = assignmnetResult.Data.Id });
    }


    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Delete(int assignmentAnswerId)
    {
        var assignmentAnswerResult = await _assignmentAnswerService.GetById(assignmentAnswerId);
        var asignmentId = assignmentAnswerResult.Data.UserAssignment.AssignmentId;

        if(!assignmentAnswerResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{assignmentAnswerResult.Data}");
            return RedirectToAction("Index", "Group");
        }

        var deleteResult = await _assignmentAnswerService.DeleteAssignmentAnswer(assignmentAnswerResult.Data);

        if (!deleteResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", deleteResult.Message);
        }

        return RedirectToAction("Details", "Assignment", new { assignmentId = asignmentId });
    }


    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> SeeStudentAnswers(int assignmentId)
    {
        var assignmentResult = await _assignmentService.GetById(assignmentId);

        if(!assignmentResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{assignmentResult.Data}");
            return RedirectToAction("Index", "Group");
        }

        var userAssignmentVMs = new List<UserAssignmentViewModel>();

        foreach (var userAssignment in assignmentResult.Data.UserAssignments)
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

        var assignmentResult = await _assignmentService.GetById(assignmentId);
        
        if(!assignmentResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{assignmentResult.Data}");
            return RedirectToAction("Index", "Group");
        }
        
        var userAssignment = assignmentResult.Data.UserAssignments.FirstOrDefault(a => a.AppUserId == student.Id);

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

        var assignmentResult = await _assignmentService.GetById(assignmentId);
        
        if(!assignmentResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{assignmentResult.Data}");
            return RedirectToAction("Index", "Group");
        }
        
        var userAssignment = assignmentResult.Data.UserAssignments.FirstOrDefault(a => a.AppUserId == studentId);

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