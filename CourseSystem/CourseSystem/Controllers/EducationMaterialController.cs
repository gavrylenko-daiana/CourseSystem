using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers;

public class EducationMaterialController : Controller
{
    private readonly IEducationMaterialService _educationMaterialService;
    private readonly IGroupService _groupService;
    private readonly ICourseService _courseService;

    public EducationMaterialController(IEducationMaterialService educationMaterial, IGroupService groupService,
        ICourseService courseService)
    {
        _educationMaterialService = educationMaterial;
        _groupService = groupService;
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var materials = await _educationMaterialService.GetAllMaterialAsync();

        if (!(materials.IsSuccessful && materials.Data.Any()))
        {
            TempData.TempDataMessage("Error", $"Message: {materials.Message}");

            return RedirectToAction("Index", "Course");
        }

        return View(materials.Data);
    }

    [HttpGet]
    public async Task<IActionResult> CreateInGroup(int groupId)
    {
        var groupResult = await _groupService.GetById(groupId);

        if (!groupResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message - {groupResult.Message}");
            return RedirectToAction("Index", "Group");
        }

        var materialViewModel = new CreateInGroupEducationMaterialViewModel
        {
            Group = groupResult.Data,
            GroupId = groupResult.Data.Id
        };

        return View(materialViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInGroup(CreateInGroupEducationMaterialViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData.TempDataMessage("Error", "Incorrect data. Please try again!");
            return View(viewModel);
        }

        var addResult = await _courseService.AddEducationMaterial(viewModel.UploadFile, viewModel.MaterialAccess, viewModel.GroupId);

        if (!addResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message: {addResult.Message}");
            return View(viewModel);
        }

        return RedirectToAction("Details", "Group", new { id = viewModel.GroupId });
    }

    [HttpGet]
    public async Task<IActionResult> CreateInCourse(int courseId)
    {
        var courseResult = await _courseService.GetById(courseId);

        if (!courseResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message - {courseResult.Message}");
            return RedirectToAction("Index", "Course");
        }

        var materialViewModel = new CreateInCourseEducationMaterialViewModel
        {
            Course = courseResult.Data,
            CourseId = courseResult.Data.Id,
            Groups = courseResult.Data.Groups
        };

        return View(materialViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInCourse(CreateInCourseEducationMaterialViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData.TempDataMessage("Error", "Incorrect data. Please try again!");
            return View(viewModel);
        }

        var addResult = await _courseService.AddEducationMaterial(viewModel.UploadFile, viewModel.MaterialAccess,
            viewModel.SelectedGroupId, viewModel.CourseId);

        if (!addResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message: {addResult.Message}");
            return View(viewModel);
        }

        return RedirectToAction("Details", "Course", new { id = viewModel.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> CreateInGeneral()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        var groups = await _groupService.GetAllGroupsAsync();

        var materialViewModel = new CreateInGeneralEducationMaterialViewModel
        {
            Courses = courses.Data!,
            Groups = groups.Data!
        };
        
        return View(materialViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInGeneral(CreateInGeneralEducationMaterialViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData.TempDataMessage("Error", "Incorrect data. Please try again!");
            return View(viewModel);
        }

        var addResult = await _courseService.AddEducationMaterial(viewModel.UploadFile, viewModel.MaterialAccess,
            viewModel.SelectedGroupId, viewModel.SelectedCourseId);

        if (!addResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message: {addResult.Message}");
            return View(viewModel);
        }

        return RedirectToAction("Index", "Course");
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var material = await _educationMaterialService.GetByIdMaterialAsync(id);

        if (!material.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message: {material.Message}");

            return RedirectToAction("Index", "Course");
        }

        TempData["UploadResult"] = material.Data.Url;

        return View(material.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var fileToDelete = await _educationMaterialService.GetByIdMaterialAsync(id);

        if (!fileToDelete.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message: {fileToDelete.Message}");

            return RedirectToAction("Detail", "EducationMaterial", new { id = id });
        }

        var deleteResult = await _educationMaterialService.DeleteFileFromGroup(fileToDelete.Data);

        await _groupService.UpdateGroup(deleteResult.Data);

        if (!deleteResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message: {deleteResult.Message}");

            return RedirectToAction("Detail", "EducationMaterial", new { id = id });
        }

        return RedirectToAction("Index", "Course");
    }
}