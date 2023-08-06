using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers
{    
    [Authorize]
    [CustomFilterAttributeException]
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
        public async Task<IActionResult> Index(int assignmentId) //here is passing group id
        {
            var groupAssignmentsResult = await _assignmentService.GetGroupAssignments(assignmentId);

            if (!groupAssignmentsResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", groupAssignmentsResult.Message);
                return RedirectToAction("Index", "Home");
            }

            var assignmentsVM = new List<AssignmentViewModel>();
            
            if (groupAssignmentsResult.Data != null)
            {
                groupAssignmentsResult.Data.ForEach(assignment =>
                {
                    var assignmentVM = new AssignmentViewModel();
                    assignment.MapTo<Assignment, AssignmentViewModel>(assignmentVM);
                    assignmentVM.UserAssignment = assignment.UserAssignments.FirstOrDefault(ua => ua.AssignmentId == assignment.Id);
                    assignmentsVM.Add(assignmentVM);
                });
            }

            ViewBag.GroupId = assignmentId;
            return View(assignmentsVM);
        }

        [HttpGet]        
        [Authorize(Roles = "Teacher")]
        [Route("Create/{groupId}")]
        public async Task<IActionResult> Create(int groupId)
        {
            var assignmentVM = new CreateAssignmentViewModel() { GroupId = groupId };

            return View(assignmentVM);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentViewModel assignmentVM) //MARCDOWN for description
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

            return RedirectToAction("Index", "Assignment", new { assignmentId = assignmentVM .GroupId});
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteAssignment(int assignmentId)
        {
            try
            {
                var assignment = await _assignmentService.GetById(assignmentId);

                var assignentDeleteVM = new DeleteAssignmentViewModel();
                assignment.MapTo<Assignment, DeleteAssignmentViewModel>(assignentDeleteVM);

                return View(assignentDeleteVM);
            }
            catch (Exception ex)
            {
                throw new Exception("Assignment not fount");
            }           
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteResult = await _assignmentService.DeleteAssignment(id);

            if (!deleteResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", deleteResult.Message);
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var assignment = await _assignmentService.GetById(id);

                var assignentDetailsVM = new DetailsAssignmentViewModel();
                assignment.MapTo<Assignment, DetailsAssignmentViewModel>(assignentDetailsVM);
                var userAssignmnet = assignment.UserAssignments.FirstOrDefault(ua => ua.AssignmentId == assignment.Id);
                //var assignmentAnswers = userAssignmnets.Select(ua => ua.AssignmentAnswers).ToList();
                assignentDetailsVM.UserAssignment = userAssignmnet;

                if (userAssignmnet?.AssignmentAnswers == null)
                    assignentDetailsVM.AssignmentAnswers = new List<AssignmentAnswer>();
                else
                    assignentDetailsVM.AssignmentAnswers = userAssignmnet.AssignmentAnswers;

                //logic for getting assignmnet files 
                assignentDetailsVM.AttachedFiles = new List<IFormFile>();

                return View(assignentDetailsVM);
            }
            catch (Exception ex)
            {
                throw new Exception("Assignment not fount");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var assignment = await _assignmentService.GetById(id);
                var assigmentVM = new EditAssignmentViewModel();
                assignment.MapTo<Assignment, EditAssignmentViewModel>(assigmentVM);

                var fileCheckBoxes = new List<FileCheckBoxViewModel>();
                foreach (var assignmentFile in assignment.AssignmentFiles)
                {
                    var checkbox = new FileCheckBoxViewModel
                    {
                        IsActive = true,
                        Description = $"{assignmentFile.Name}",
                        Value = assignmentFile
                    };

                    fileCheckBoxes.Add(checkbox);
                }

                assigmentVM.AttachedFilesCheckBoxes = fileCheckBoxes;

                return View(assigmentVM);

            }
            catch (Exception ex)
            {
                throw new Exception("Assignment not fount");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditAssignmentViewModel editAssignmentVM)
        {
            if (editAssignmentVM == null)
                return View("Error");

            if(!ModelState.IsValid)
            {
                TempData.TempDataMessage("Error", "Invalid data input");
                return View(editAssignmentVM.Id);
            }
           
            var assignment = new Assignment();
            editAssignmentVM.MapTo<EditAssignmentViewModel, Assignment>(assignment);

            //AssignmentFiles Part
            //logic for check if the checkbox files was in the assignmnet before

            //logic fore saving new attached files

            var updateAssignmnetResult = await _assignmentService.UpdateAssignment(assignment);

            if (!updateAssignmnetResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", updateAssignmnetResult.Message);
                return View(editAssignmentVM.Id);
            }

            return RedirectToAction("Index", "Assignment", new {editAssignmentVM.GroupId });
        }
    }
}
