using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UI.ViewModels;

namespace UI.Controllers
{
    [CustomFilterAttributeException]

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
        [Route("CreateAnswer/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateAnswer(int id)
        {
            try
            {
                var assignmnetAnsweVM = new AssignmentAnsweViewModel()
                {
                    AssignmentId = id
                };
               
                return View(assignmnetAnsweVM);
            }
            catch (Exception ex)
            {
                throw new Exception("Assignment doesn't found");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(AssignmentAnsweViewModel assignmentAnswerVM)
        {
            if(!ModelState.IsValid)
                return View(assignmentAnswerVM);

            var assignmnetAnswer = new AssignmentAnswer();
            assignmentAnswerVM.MapTo<AssignmentAnsweViewModel, AssignmentAnswer>(assignmnetAnswer);

            if (!assignmentAnswerVM.AssignmentAnswerFiles.IsNullOrEmpty())
            {
                //logic for files
                //set the name of file to model
            }

            assignmnetAnswer.Name = assignmentAnswerVM.AnswerDescription;
            assignmnetAnswer.Text = assignmentAnswerVM.AnswerDescription; //markdown
            assignmnetAnswer.Url = "Some URL";

            var assignmnet = await _assignmentService.GetById(assignmentAnswerVM.AssignmentId);
            var currentUser = await _userManager.GetUserAsync(User);

            var answerResult = await _assignmentAnswerService.CreateAssignmentAnswer(assignmnetAnswer, assignmnet, currentUser);

            if (!answerResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", "Fail to save assignmnet answer");
                return RedirectToAction("CreateAnswer", "AssignmentAnswer", new { assignmentAnswerVM.AssignmentId });
            }

            return RedirectToAction("Details", "Assignment", new {id = assignmnet.Id});
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Delete(int assignmentAnswerId)
        {
            var assignmentAnswer = await _assignmentAnswerService.GetById(assignmentAnswerId);
            var asignmentId = assignmentAnswer.UserAssignment.AssignmentId;

            if (assignmentAnswer == null)
                return NotFound();

            var deleteResult = await _assignmentAnswerService.DeleteAssignmentAnswer(assignmentAnswer);

            if(!deleteResult.IsSuccessful)
                TempData.TempDataMessage("Error", deleteResult.Message);

            return RedirectToAction("Details", "Assignment", new { id = asignmentId });
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> SeeStudentAnswers(int assignmentId)
        {
            var assignment = await _assignmentService.GetById(assignmentId);

            if(assignment == null)
                return NotFound();

            var userAssignmentVMs = new List<UserAssignmentViewModel>();

            foreach(var userAssignment in assignment.UserAssignments)
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
                return NotFound();

            var assignment = await _assignmentService.GetById(assignmentId);
            var userAssignment = assignment.UserAssignments.FirstOrDefault(a => a.AppUserId ==  student.Id);
            
            if(userAssignment == null)
                return NotFound();

            var checkAnswerVM = new CheckAnswerViewModel();
            userAssignment.MapTo<UserAssignments,  CheckAnswerViewModel>(checkAnswerVM);

            return View(checkAnswerVM);
        }
    }
}
