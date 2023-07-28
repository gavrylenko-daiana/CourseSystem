using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers
{
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
        [Route("Create/{groupId}")]
        public async Task<IActionResult> Create(int groupId)
        {
            //check if this group exist 

            var assignmentVM = new CreateAssignmentViewModel() { GroupId = groupId };

            return View(assignmentVM);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentViewModel assignmentVM)
        {
            if(assignmentVM == null)
                return View("Error");

            if(!ModelState.IsValid)
            {
                TempData.TempDataMessage("Error", "Failed to create assignment");
                return View("CreateAssignment", assignmentVM);
            }

            var assignment = new Assignment();
            LibraryForMapping.MapTo<CreateAssignmentViewModel, Assignment>(assignmentVM, assignment);

            return View();
        }
    }
}
