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
    [Authorize(Roles = "Student")]
    public class AssignmentAnswerController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAssignmentService _assignmentService;
        private readonly IAssignmentAnswerService _assignmentAnswerService;
        public AssignmentAnswerController(UserManager<AppUser> userManager,
            IAssignmentService assignmentService,
            IAssignmentAnswerService assignmentAnswerService)
        {
            _userManager = userManager;
            _assignmentService = assignmentService;
            _assignmentAnswerService = assignmentAnswerService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("CreateAnswer/{id}")]
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
    }
}
