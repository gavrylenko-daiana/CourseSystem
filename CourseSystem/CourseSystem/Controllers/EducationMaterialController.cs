using BLL.Interfaces;
using CloudinaryDotNet.Actions;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers;

public class EducationMaterialController : Controller
{
    private readonly IEducationMaterialService _educationMaterialService;
    private readonly IGroupService _groupService;

    public EducationMaterialController(IEducationMaterialService educationMaterial, IGroupService groupService)
    {
        _educationMaterialService = educationMaterial;
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var materials = await _educationMaterialService.GetAllMaterialAsync();

        return View(materials);
    }

    [HttpGet]
    public async Task<IActionResult> CreateInGroup(int groupId)
    {
        var group = await _groupService.GetById(groupId);
        
        var materialViewModel = new CreateEducationMaterialViewModel
        {
            CourseId = group.CourseId,
            GroupId = groupId
        };

        return View(materialViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInGroup(CreateEducationMaterialViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData.TempDataMessage("Error", "Incorrect data. Please try again!");

            return View(viewModel);
        }
        
        UploadResult uploadResult = await _educationMaterialService.AddFileAsync(viewModel.UploadFile);

        await _educationMaterialService.AddToGroup(viewModel.UploadFile, viewModel.GroupId, uploadResult.Url.ToString());
        
        return RedirectToAction("Index", "Course");
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var material = await _educationMaterialService.GetByIdMaterialAsync(id);

        TempData["UploadResult"] = material.Url;

        return View(material);
    }
    
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var fileToDelete = await _educationMaterialService.GetByIdMaterialAsync(id);

        if (fileToDelete == null)
        {
            return NotFound();
        }
        
        var access = fileToDelete.MaterialAccess;

        switch (access)
        {
            case MaterialAccess.Group:
                await _educationMaterialService.DeleteFileAsync(fileToDelete.Url);
                await _educationMaterialService.DeleteUploadFileAsync(fileToDelete);
                
                var group = fileToDelete.Group;
                
                if (group != null)
                {
                    group.EducationMaterials.Remove(fileToDelete);
                    await _groupService.UpdateGroup(group);
                }
                break;
            case MaterialAccess.Course:
                
                break;
            case MaterialAccess.General:
                
                break;
            default:
                break;
        }

        return RedirectToAction("Index", "Course");
    }
}