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
        // check result
        var group = await _groupService.GetById(groupId);

        var materialViewModel = new CreateInGroupEducationMaterialViewModel
        {
            Group = group,
            GroupId = group.Id
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
        
        var group = await _groupService.GetById(viewModel.GroupId);

        if (group == null)
        {
            TempData.TempDataMessage("Error", $"Failed to get {nameof(group)} by id {viewModel.GroupId}");
            return RedirectToAction("Index", "Course");
        }

        var addResult = await _educationMaterialService.AddToGroup(viewModel.UploadFile,
            fullPath.Message, group);
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
        // check result
        var course = await _courseService.GetById(courseId);
    
        var materialViewModel = new CreateInCourseEducationMaterialViewModel
        {
            Course = course,
            CourseId = course.Id,
            Groups = course.Groups
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
        
        var group = await _groupService.GetById(viewModel.SelectedGroupId);

        if (group == null)
        {
            TempData.TempDataMessage("Error", $"Failed to get {nameof(group)} by id {viewModel.SelectedGroupId}");
            return RedirectToAction("Index", "Course");
        }

        if (viewModel.MaterialAccess == MaterialAccess.Group)
        {
            var addToGroupResult = await _educationMaterialService.AddToGroup(viewModel.UploadFile,
                fullPath.Message, group);
            
            if (!addToGroupResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", $"Message: {addToGroupResult.Message}");
                return View(viewModel);
            }
        }
        else
        {
            var course = await _courseService.GetById(viewModel.CourseId);

            if (course == null)
            {
                TempData.TempDataMessage("Error", $"Failed to get {nameof(course)} by id {viewModel.CourseId}");
                return RedirectToAction("Index", "Course");
            }

            var addToCourseResult = await _educationMaterialService.AddToCourse(viewModel.UploadFile,
                fullPath.Message, course);
            
            await _courseService.UpdateCourse(course);
            
            if (!addToCourseResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", $"Message: {addToCourseResult.Message}");
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