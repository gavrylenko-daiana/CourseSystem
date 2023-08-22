using BLL.Interfaces;
using Core.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers;

public class AssignmentFileController : Controller
{
    private readonly IAssignmentFileService _assignmentFileService;

    public AssignmentFileController(IAssignmentFileService assignmentFileService)
    {
        _assignmentFileService = assignmentFileService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int fileId)
    {
        var assignmentFile = await _assignmentFileService.GetById(fileId);

        if (!assignmentFile.IsSuccessful)
        {
            TempData.TempDataMessage("Error", assignmentFile.Message);

            return RedirectToAction("Index", "Group");
        }
        
        return View(assignmentFile.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int fileId)
    {
        return View();
    }
}