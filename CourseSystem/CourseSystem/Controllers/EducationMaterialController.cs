using System.Text.Json;
using System.Text.Json.Serialization;
using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;
using static Dropbox.Api.Team.GroupSelector; //I don't know what that is, need to check
using UI.ViewModels.FileViewModels;
using BLL.Services;
using Core.EmailTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using X.PagedList;
using System.Security.Policy;
using UI.ViewModels.EmailViewModels;

namespace UI.Controllers;

[Authorize]
public class EducationMaterialController : Controller
{
    private readonly IEducationMaterialService _educationMaterialService;
    private readonly IGroupService _groupService;
    private readonly ICourseService _courseService;
    private readonly IActivityService _activityService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IDropboxService _dropboxService;
    private readonly ILogger<EducationMaterialController> _logger;

    public EducationMaterialController(IEducationMaterialService educationMaterial, IGroupService groupService,
        ICourseService courseService, IActivityService activityService,
        IUserService userService, IDropboxService dropboxService, ILogger<EducationMaterialController> logger, IEmailService emailService)
    {
        _educationMaterialService = educationMaterial;
        _groupService = groupService;
        _courseService = courseService;
        _activityService = activityService;
        _userService = userService;
        _dropboxService = dropboxService;
        _logger = logger;
        _emailService = emailService;
    }
    
    private string CreateCallBackUrl(string controllerName, string actionName, object routeValues)
    {
        if (controllerName.IsNullOrEmpty() || actionName.IsNullOrEmpty())
        {
            return Url.ActionLink("Index", "Home", protocol: HttpContext.Request.Scheme) ?? string.Empty;
        }

        if (routeValues == null)
        {
            return Url.ActionLink("Index", "Home", protocol: HttpContext.Request.Scheme) ?? string.Empty;
        }

        var callbackUrl = Url.Action(
            actionName,
            controllerName,
            routeValues,
            protocol: HttpContext.Request.Scheme);

        return callbackUrl;
    }

    private async Task SendEmailToAdminToApprove(string teacherId, IFormFile formFile, MaterialAccess materialAccess, AppUser toUser, int? courseId, int? groupId)
    {
        var dropboxTempUploadResult = await _dropboxService.AddFileAsync(formFile, materialAccess.ToString());

        if (dropboxTempUploadResult.IsSuccessful)
        {
            var callBack = CreateCallBackUrl("EducationMaterial", "EmailConfirmationUploadMaterialByAdmin",
            new
            {
                teacherId = teacherId,
                url = dropboxTempUploadResult.Data.Url,
                name = dropboxTempUploadResult.Data.ModifiedFileName,
                materialAccess = materialAccess,
                courseId = courseId,
                groupId = groupId
            });

            var emailResult = await _emailService.SendEmailToAppUsers(EmailType.EducationMaterialApproveByAdmin, toUser, callBack, null, formFile);

            if (!emailResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", emailResult.Message);
            }

            TempData.TempDataMessage("Error", "Wait for the approve from admin");
        }
        else
        {
            TempData.TempDataMessage("Error", dropboxTempUploadResult.Message);
        }
        
    }
    
    [HttpGet]
    public async Task<IActionResult> IndexMaterials(string materialIds, SortingParam sortOrder, string currentQueryFilter, string searchQuery, int? page)
    {
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParam = sortOrder == SortingParam.UploadTimeDesc ? SortingParam.UploadTime : SortingParam.UploadTimeDesc;

        if (searchQuery != null)
        {
            page = 1;
        }
        else
        {
            searchQuery = currentQueryFilter;
        }
        
        ViewBag.CurrentQueryFilter = searchQuery;
        ViewBag.MaterialIds = materialIds;
        
        var materialsList = await _educationMaterialService.GetMaterialsListFromIdsString(materialIds, sortOrder, searchQuery);
        
        if (!materialsList.IsSuccessful)
        {
            TempData.TempDataMessage("Error", materialsList.Message);
            
            return RedirectToAction("Index", "Course");
        }

        int pageSize = 8;
        int pageNumber = (page ?? 1);
        ViewBag.OnePageOfAssignemnts = materialsList.Data;

        return View("Index", materialsList.Data.ToPagedList(pageNumber, pageSize));
    }

    [HttpGet]
    public async Task<IActionResult> CreateInGroup(int groupId, bool isApproved = false)
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
            GroupId = groupResult.Data.Id,
            Course = groupResult.Data.Course,
            CourseId = groupResult.Data.CourseId
        };

        ViewBag.IsApproved = isApproved;

        return View(materialViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInGroup(CreateInGroupEducationMaterialViewModel viewModel)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);
        
        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

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

        if (currentUserResult.Data.Role == AppUserRoles.Teacher)
        {
            await SendEmailToAdminToApprove(currentUserResult.Data.Id, viewModel.UploadFile, viewModel.MaterialAccess, currentUserResult.Data, viewModel.CourseId, viewModel.GroupId);

            return RedirectToAction("CreateInGroup", "EducationMaterial", new { groupId = viewModel.GroupId });
        }
        else
        {
            var groupResult = await _groupService.GetById(viewModel.GroupId);

            if (!groupResult.IsSuccessful)
            {
                _logger.LogError("Failed to get group by Id {groupId}! Error: {errorMessage}",
                    viewModel.GroupId, groupResult.Message);

                TempData.TempDataMessage("Error", $"Message: {groupResult.Message}");

                return RedirectToAction("CreateInGroup", "EducationMaterial", new { groupId = viewModel.GroupId });
            }

            var addResult = await _courseService.AddEducationMaterial(viewModel.TimeUploaded, viewModel.UploadFile, viewModel.MaterialAccess, viewModel.GroupId, viewModel.CourseId);

            if (!addResult.IsSuccessful)
            {
                _logger.LogError("Failed to upload educational material! Error: {errorMessage}", addResult.Message);

                TempData.TempDataMessage("Error", $"Message: {addResult.Message}");

                return RedirectToAction("CreateInGroup", "EducationMaterial", new { groupId = viewModel.GroupId });
            }

            await _activityService.AddAttachedEducationalMaterialForGroupActivity(currentUserResult.Data, groupResult.Data);

            return RedirectToAction("Details", "Group", new { id = viewModel.GroupId });
        }        
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
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        if (currentUserResult.Data.Role == AppUserRoles.Teacher)
        {
            await SendEmailToAdminToApprove(currentUserResult.Data.Id, viewModel.UploadFile, viewModel.MaterialAccess, currentUserResult.Data, viewModel.CourseId, viewModel.SelectedGroupId);
          
            return RedirectToAction("CreateInCourse", "EducationMaterial", new { courseId = viewModel.CourseId });
        }
        else
        {
            var courseResult = await _courseService.GetById(viewModel.CourseId);

            if (!courseResult.IsSuccessful)
            {
                _logger.LogError("Failed to get course by Id {courseId}! Error: {errorMessage}",
                    viewModel.CourseId, courseResult.Message);

                TempData.TempDataMessage("Error", $"Message: {courseResult.Message}");

                return RedirectToAction("CreateInCourse", "EducationMaterial", new { courseId = viewModel.CourseId });
            }

            var addResult = await _courseService.AddEducationMaterial(viewModel.TimeUploaded, viewModel.UploadFile,
                viewModel.MaterialAccess, viewModel.SelectedGroupId, viewModel.CourseId);

            if (!addResult.IsSuccessful)
            {
                _logger.LogError("Failed to upload educational material! Error: {errorMessage}", addResult.Message);

                TempData.TempDataMessage("Error", $"Message: {addResult.Message}");

                return RedirectToAction("CreateInCourse", "EducationMaterial", new { courseId = viewModel.CourseId });
            }

            await _activityService.AddAttachedEducationalMaterialForCourseActivity(currentUserResult.Data, courseResult.Data);
        }

        return RedirectToAction("Details", "Course", new { id = viewModel.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> EmailConfirmationUploadMaterialByAdmin(string teacherId, string url, string name, MaterialAccess materialAccess, int? courseId = null, int? groupId = null) //crete view for =  admin about success
    {
        var teacherResult = await _userService.FindByIdAsync(teacherId);

        if (!teacherResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", teacherResult.Message);
        }

        var emailViewModel = new ConfirmEducationMaterial()
        {
            TeacherId = teacherId,
            FileUrl = url,
            FileName = name,
            MaterialAccess = materialAccess,
            CourseId = courseId,
            GroupId = groupId
        };     

        return View(emailViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(ConfirmEducationMaterial educationMaterialVM)
    {
        var groupResult = await _groupService.GetById((int)educationMaterialVM.GroupId);
        var courseResult = await _courseService.GetById((int)educationMaterialVM.CourseId);
        var addEducationMaterialResult = await _educationMaterialService.AddEducationMaterial(DateTime.Now, educationMaterialVM.FileName, educationMaterialVM.FileUrl, educationMaterialVM.MaterialAccess, groupResult.Data, courseResult.Data);

        if (!addEducationMaterialResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", addEducationMaterialResult.Message);

            return RedirectToAction("EmailConfirmationUploadMaterialByAdmin", "EducationMaterial", new
            {
                teacherId = educationMaterialVM.TeacherId,
                url = educationMaterialVM.FileUrl,
                name = educationMaterialVM.FileName,
                materialAccess = educationMaterialVM.MaterialAccess,
                courseId = educationMaterialVM.CourseId,
                groupId = educationMaterialVM.GroupId
            });
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFile(ConfirmEducationMaterial educationMaterialVM)
    {
        var approveForDelete = await _educationMaterialService.ApprovedEducationMaterial(educationMaterialVM.FileName, educationMaterialVM.FileUrl);

        if (!approveForDelete.IsSuccessful)
        {
            TempData.TempDataMessage("Error", approveForDelete.Message);

            return RedirectToAction("EmailConfirmationUploadMaterialByAdmin", "EducationMaterial", new
            {
                teacherId = educationMaterialVM.TeacherId,
                url = educationMaterialVM.FileUrl,
                name = educationMaterialVM.FileName,
                materialAccess = educationMaterialVM.MaterialAccess,
                courseId = educationMaterialVM.CourseId,
                groupId = educationMaterialVM.GroupId
            });
        }
        
        var exureFileExist = await _dropboxService.FileExistsAsync(educationMaterialVM.FileName, educationMaterialVM.MaterialAccess.ToString());

        if (!exureFileExist.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"File {educationMaterialVM.FileName} was denyed");

            return RedirectToAction("EmailConfirmationUploadMaterialByAdmin", "EducationMaterial", new
            {
                teacherId = educationMaterialVM.TeacherId,
                url = educationMaterialVM.FileUrl,
                name = educationMaterialVM.FileName,
                materialAccess = educationMaterialVM.MaterialAccess,
                courseId = educationMaterialVM.CourseId,
                groupId = educationMaterialVM.GroupId
            });
        }

        var deletionResult = await _dropboxService.DeleteFileAsync(educationMaterialVM.FileName, educationMaterialVM.MaterialAccess.ToString());

        if (!deletionResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", deletionResult.Message);

            return RedirectToAction("EmailConfirmationUploadMaterialByAdmin", "EducationMaterial", new
            {
                teacherId = educationMaterialVM.TeacherId,
                url = educationMaterialVM.FileUrl,
                name = educationMaterialVM.FileName,
                materialAccess = educationMaterialVM.MaterialAccess,
                courseId = educationMaterialVM.CourseId,
                groupId = educationMaterialVM.GroupId
            });
        }

        return RedirectToAction("Index", "Home");
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
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        if (currentUserResult.Data.Role == AppUserRoles.Teacher)
        {
            await SendEmailToAdminToApprove(currentUserResult.Data.Id, viewModel.UploadFile, viewModel.MaterialAccess, currentUserResult.Data, viewModel.SelectedCourseId, viewModel.SelectedGroupId);

            return RedirectToAction("CreateInGeneral", "EducationMaterial");
        }
        else
        {
            var addResult = await _courseService.AddEducationMaterial(viewModel.TimeUploaded, viewModel.UploadFile, viewModel.MaterialAccess,
            viewModel.SelectedGroupId, viewModel.SelectedCourseId);

            if (!addResult.IsSuccessful)
            {
                _logger.LogError("Failed to upload educational material! Error: {errorMessage}", addResult.Message);

                TempData.TempDataMessage("Error", $"Message: {addResult.Message}");

                return RedirectToAction("CreateInGeneral", "EducationMaterial");
            }

            await _activityService.AddAttachedGeneralEducationalMaterialActivity(currentUserResult.Data);

            return RedirectToAction("Index", "Course");
        }
    }

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

        var deleteResult = await _educationMaterialService.DeleteFile(fileToDelete.Data);

        if (!deleteResult.IsSuccessful)
        {
            _logger.LogError("Failed to delete educational material by Id {materialId}! Error: {errorMessage}",
                fileToDelete.Data.Id, deleteResult.Message);

            TempData.TempDataMessage("Error", $"Message: {deleteResult.Message}");

            return RedirectToAction("Detail", "EducationMaterial", new { id = id });
        }

        return RedirectToAction("Index", "Course");
    }
}