using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers
{
    [CustomFilterAttributeException]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        public AssignmentController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;   
        }
        public async Task<IActionResult> Index(int id) //here is passing group id
        {
            //all group assignments view (for student - InProgress + AwaitedApproval, for teachers - All statuses) 
            //button for cretion of new one

            return View();
        }

        [HttpGet]        
        [Authorize(Roles = "Teacher")]
        //[Route("Create/{id}")]
        public async Task<IActionResult> Create()
        {
            //check if this group exist 

            //for tests
            int id = 1;

            var assignmentVM = new CreateAssignmentViewModel() { GroupId = id };

            return View(assignmentVM);
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentViewModel assignmentVM)
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
    }
}
