using BLL.Interfaces;
using Core.Enums;
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
            return RedirectToAction("MessageForNonexistentEntity", "General", new { entityType = EntityType.AssignmentFile });
        }
        
        return View(assignmentFile.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int fileId, int assignmentId)
    {
        var deleteFileResult = await _assignmentFileService.DeleteAssignmentFile(fileId);

        if (!deleteFileResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", deleteFileResult.Message);
        }

        return RedirectToAction("Details", "Assignment", new { assignmentId = assignmentId });
    }
}