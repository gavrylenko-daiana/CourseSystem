using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;
using static Dropbox.Api.Team.GroupSelector;

namespace UI.Controllers;

public class EducationMaterialController : Controller
{
    private readonly IEducationMaterialService _educationMaterialService;
    private readonly IGroupService _groupService;
    private readonly ICourseService _courseService;
    private readonly ILogger<EducationMaterialController> _logger;

    public EducationMaterialController(IEducationMaterialService educationMaterial, IGroupService groupService,
        ICourseService courseService, ILogger<EducationMaterialController> logger)
    {
        _educationMaterialService = educationMaterial;
        _groupService = groupService;
        _courseService = courseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var materials = await _educationMaterialService.GetAllMaterialAsync();

        if (!(materials.IsSuccessful && materials.Data.Any()))
        {
            _logger.LogError("Failed to retrieve educational materials! Error: {errorMessage}", materials.Message);

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
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                groupId, groupResult.Message);

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
            _logger.LogError("Failed to create eduactional material! Invalid model state.");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("Error: {errorMessage}", error.ErrorMessage);
            }

            TempData.TempDataMessage("Error", "Incorrect data. Please try again!");

            return View(viewModel);
        }

        var fullPath = await _educationMaterialService.AddFileAsync(viewModel.UploadFile);

        if (!fullPath.IsSuccessful)
        {
            _logger.LogError("Failed to upload educational material! Error: {errorMessage}", fullPath.Message);
            TempData.TempDataMessage("Error", $"Message: {fullPath.Message}");

            return View(viewModel);
        }

        var groupResult = await _groupService.GetById(viewModel.GroupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                viewModel.GroupId, groupResult.Message);

            TempData.TempDataMessage("Error", $"Message - {groupResult.Message}");

            return RedirectToAction("Index", "Group");
        }

        var addResult = await _educationMaterialService.AddToGroup(viewModel.UploadFile, fullPath.Message, groupResult.Data);

        if (!addResult.IsSuccessful)
        {
            _logger.LogError("Failed to attach educational material {filePath} to group {groupId}! Error: {errorMessage}", 
                fullPath.Data, groupResult.Data.Id, addResult.Message);
            TempData.TempDataMessage("Error", $"Message: {addResult.Message}");

            return View(viewModel);
        }

        var updateResult = await _groupService.UpdateGroup(addResult.Data);

        if (!updateResult.IsSuccessful)
        {
            _logger.LogError("Failed to update group by Id {groupId}! Error: {errorMessage}",
                addResult.Data.Id, updateResult.Message);
            TempData.TempDataMessage("Error", $"Message: {updateResult.Message}");

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
            _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                courseId, courseResult.Message);

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
        var fullPath = await _educationMaterialService.AddFileAsync(viewModel.UploadFile);

        if (!fullPath.IsSuccessful)
        {
            _logger.LogError("Failed to upload educational material! Error: {errorMessage}", fullPath.Message);
            TempData.TempDataMessage("Error", $"Message: Failed to upload file");

            return View(viewModel);
        }

        var groupResult = await _groupService.GetById(viewModel.SelectedGroupId);

        if (!groupResult.IsSuccessful)
        {
            _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                viewModel.SelectedGroupId, groupResult.Message);

            TempData.TempDataMessage("Error", $"Message - {groupResult.Message}");

            return RedirectToAction("Index", "Group");
        }

        if (viewModel.MaterialAccess == MaterialAccess.Group)
        {
            var addToGroupResult = await _educationMaterialService.AddToGroup(viewModel.UploadFile, fullPath.Message, groupResult.Data);

            if (!addToGroupResult.IsSuccessful)
            {
                _logger.LogError("Failed to attach educational material {filePath} to group {groupId}! Error: {errorMessage}",
                    fullPath.Data, groupResult.Data.Id, addToGroupResult.Message);
                TempData.TempDataMessage("Error", $"Message: {addToGroupResult.Message}");

                return View(viewModel);
            }
        }
        else
        {
            var courseResult = await _courseService.GetById(viewModel.CourseId);

            if (!courseResult.IsSuccessful)
            {
                _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                    viewModel.CourseId, courseResult.Message);

                TempData.TempDataMessage("Error", $"Message - {courseResult.Message}");

                return RedirectToAction("Index", "Course");
            }

            var addToCourseResult = await _educationMaterialService.AddToCourse(viewModel.UploadFile,
                fullPath.Message, courseResult.Data);
            
            if (!addToCourseResult.IsSuccessful)
            {
                _logger.LogError("Failed to attach educational material {filePath} to course {courseId}! Error: {errorMessage}",
                    fullPath.Data, courseResult.Data.Id, addToCourseResult.Message);

                TempData.TempDataMessage("Error", $"Message: {addToCourseResult.Message}");

                return View(viewModel);
            }
            
            var updateCourseResult = await _courseService.UpdateCourse(courseResult.Data);

            if (!updateCourseResult.IsSuccessful)
            {
                _logger.LogError("Failed to update course by Id {courseId}! Error: {errorMessage}",
                    courseResult.Data.Id, updateCourseResult.Message);

                TempData.TempDataMessage("Error", $"Message: {updateCourseResult.Message}");
                return View(viewModel);
            }
        }

        return RedirectToAction("Details", "Course", new { id = viewModel.CourseId });
    }

    // [HttpGet]
    // public async Task<IActionResult> CreateInGeneral()
    // {
    //     var courses = await _courseService.GetAllCoursesAsync();
    //     var groups = await _groupService.GetAllGroupsAsync();
    //
    //     var materialViewModel = new CreateInGeneralEducationMaterialViewModel
    //     {
    //         Courses = courses.Data!,
    //         Groups = groups.Data!
    //     };
    //
    //     return View(materialViewModel);
    // }
    //
    // [HttpPost]
    // public async Task<IActionResult> CreateInGeneral(CreateInGeneralEducationMaterialViewModel viewModel)
    // {
    //     var fullPath = await _educationMaterialService.AddFileAsync(viewModel.UploadFile);
    //
    //     if (!fullPath.IsSuccessful)
    //     {
    //         TempData.TempDataMessage("Error", $"Message: Failed to upload file");
    //         return View(viewModel);
    //     }
    //
    //     if (viewModel.MaterialAccess == MaterialAccess.Group)
    //     {
    //         var addToGroupResult = await _educationMaterialService.AddToGroup(viewModel.UploadFile,
    //             fullPath.Message, viewModel.SelectedGroupId);
    //         
    //         if (!addToGroupResult.IsSuccessful)
    //         {
    //             TempData.TempDataMessage("Error", $"Message: {addToGroupResult.Message}");
    //             return View(viewModel);
    //         }
    //     }
    //     else
    //     {
    //         var addToCourseResult = await _educationMaterialService.AddToCourse(viewModel.UploadFile,
    //             fullPath.Message, viewModel.CourseId);
    //         
    //         if (!addToCourseResult.IsSuccessful)
    //         {
    //             TempData.TempDataMessage("Error", $"Message: {addToCourseResult.Message}");
    //             return View(viewModel);
    //         }
    //     }
    //
    //     return RedirectToAction("Details", "Course", new { id = viewModel.CourseId });
    // }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var material = await _educationMaterialService.GetByIdMaterialAsync(id);

        if (!material.IsSuccessful)
        {
            _logger.LogError("Failed to get educational material by Id {materialId}! Error: {errorMessage}",
                id, material.Message);

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
            _logger.LogError("Failed to get educational material by Id {materialId}! Error: {errorMessage}",
                id, fileToDelete.Message);

            TempData.TempDataMessage("Error", $"Message: {fileToDelete.Message}");

            return RedirectToAction("Detail", "EducationMaterial", new { id = id });
        }

        var deleteResult = await _educationMaterialService.DeleteFileFromGroup(fileToDelete.Data);

        if (!deleteResult.IsSuccessful)
        {
            _logger.LogError("Failed to delete educational material by Id {materialId}! Error: {errorMessage}",
                fileToDelete.Data.Id, deleteResult.Message);

            TempData.TempDataMessage("Error", $"Message: {deleteResult.Message}");

            return RedirectToAction("Detail", "EducationMaterial", new { id = id });
        }

        var updateResult = await _groupService.UpdateGroup(deleteResult.Data);

        if (!updateResult.IsSuccessful)
        {
            _logger.LogError("Failed to update group by Id {groupId}! Error: {errorMessage}",
                deleteResult.Data.Id, updateResult.Message);
            TempData.TempDataMessage("Error", $"Message: {updateResult.Message}");
        }

        return RedirectToAction("Index", "Course");
    }
}