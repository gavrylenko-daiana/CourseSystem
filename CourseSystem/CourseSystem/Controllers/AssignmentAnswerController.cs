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
    [Authorize(Roles = "Student")]
    public class AssignmentAnswerController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAssignmentService _assignmentService;
        public AssignmentAnswerController(UserManager<AppUser> userManager,
            IAssignmentService assignmentService)
        {
            _userManager = userManager;
            _assignmentService = assignmentService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            try
            {
                var assignmnet = await _assignmentService.GetById(id);
                var currentUser = await _userManager.GetUserAsync(User);
            }
            catch (Exception ex)
            {
                throw new Exception("Assignment doesn't found");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(AssignmentAnsweViewModel assignmentAnsweVM)
        {

        }
    }
}
