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

    public EducationMaterialController(IEducationMaterialService educationMaterial, IGroupService groupService, ICourseService courseService)
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

        var fullPath = await _educationMaterialService.AddFileAsync(viewModel.UploadFile);

        if (!fullPath.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message: {fullPath.Message}");
            return View(viewModel);
        }
        
        var groupResult = await _groupService.GetById(viewModel.GroupId);

        if (!groupResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message - {groupResult.Message}");
            return RedirectToAction("Index", "Group");
        }

        var addResult = await _educationMaterialService.AddToGroup(viewModel.UploadFile,
            fullPath.Message, groupResult.Data);
        await _groupService.UpdateGroup(addResult.Data);

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
        var fullPath = await _educationMaterialService.AddFileAsync(viewModel.UploadFile);
    
        if (!fullPath.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message: Failed to upload file");
            return View(viewModel);
        }
        
        var groupResult = await _groupService.GetById(viewModel.SelectedGroupId);

        if (!groupResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"Message - {groupResult.Message}");
            return RedirectToAction("Index", "Group");
        }

        if (viewModel.MaterialAccess == MaterialAccess.Group)
        {
            var addToGroupResult = await _educationMaterialService.AddToGroup(viewModel.UploadFile,
                fullPath.Message, groupResult.Data);
            
            if (!addToGroupResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", $"Message: {addToGroupResult.Message}");
                return View(viewModel);
            }
        }
        else
        {
            var courseResult = await _courseService.GetById(viewModel.CourseId);
            
            if (!courseResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", $"Message - {courseResult.Message}");
                return RedirectToAction("Index", "Course");
            }

            var addToCourseResult = await _educationMaterialService.AddToCourse(viewModel.UploadFile,
                fullPath.Message, courseResult.Data);
            
            if (!addToCourseResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", $"Message: {addToCourseResult.Message}");
                return View(viewModel);
            }
            
            var updateCourseResult = await _courseService.UpdateCourse(courseResult.Data);

            if (!updateCourseResult.IsSuccessful)
            {
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