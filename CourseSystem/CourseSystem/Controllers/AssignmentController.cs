using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers
{
    [CustomFilterAttributeException]
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly UserManager<AppUser> _userManager;
        public AssignmentController(IAssignmentService assignmentService,
            UserManager<AppUser> userManager)
        {
            _assignmentService = assignmentService;   
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int id) //here is passing group id
        {
            var groupAssignmentsResult = await _assignmentService.GetGroupAssignments(id);

            if (!groupAssignmentsResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", groupAssignmentsResult.Message);
                return RedirectToAction("Index", "Home");
            }

            var assignmentsVM = new List<AssignmentViewModel>();
            foreach(var assignment in groupAssignmentsResult.Data)
            {
                var assignmentVM = new AssignmentViewModel();
                assignment.MapTo<Assignment, AssignmentViewModel>(assignmentVM);
                assignmentVM.UserAssignment = assignment.UserAssignments.FirstOrDefault(ua => ua.AssignmentId == assignment.Id);
                assignmentsVM.Add(assignmentVM);
            }
            
            return View(assignmentsVM);
        }

        [HttpGet]        
        [Authorize(Roles = "Teacher")]
        [Route("Create/{id}")]
        public async Task<IActionResult> Create(int id)
        {
            //check if this group exist 

            //for tests
            //int id = 1;

            var assignmentVM = new CreateAssignmentViewModel() { GroupId = id };

            return View(assignmentVM);
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentViewModel assignmentVM) //MARCDOWN
        {
            if(assignmentVM == null)
                return View("Error");

            if(!ModelState.IsValid)
            {
                TempData.TempDataMessage("Error", "Invalid input data");
                return View(assignmentVM);
            }

            var assignment = new Assignment();
            LibraryForMapping.MapTo<CreateAssignmentViewModel, Assignment>(assignmentVM, assignment);

            if(assignmentVM.AttachedFiles != null)
            {
                //logic for loading files to the cloud or this logic can be inside assignmentService

                TempData.TempDataMessage("Error", "Files uploaded successfully");
                return View(assignmentVM);
            }

            var createResult = await _assignmentService.CreateAssignment(assignment);

            if (!createResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", createResult.Message);
                return View(assignmentVM);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _assignmentService.GetById(id);

            if (assignment == null)
                return View("Error");

            var assignentDeleteVM = new DeleteAssignmentViewModel();
            assignment.MapTo<Assignment, DeleteAssignmentViewModel>(assignentDeleteVM);

            return View(assignentDeleteVM);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            return View("Index", "Home");
        }
    }
}
