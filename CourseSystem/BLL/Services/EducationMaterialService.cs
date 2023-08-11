using System.Net;
using BLL.Interfaces;
using Core.Configuration;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using DAL.Repository;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Team;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BLL.Services;

public class EducationMaterialService : GenericService<EducationMaterial>, IEducationMaterialService
{
    private readonly DropboxClient _dropboxClient;
    private readonly ICourseService _courseService;
    private readonly IGroupService _groupService;

    public EducationMaterialService(UnitOfWork unitOfWork, IOptions<DropboxSettings> config,
        ICourseService courseService, IGroupService groupService) : base(unitOfWork,
        unitOfWork.EducationMaterialRepository)
    {
        _dropboxClient = new DropboxClient(config.Value.AccessToken);
        _courseService = courseService;
        _groupService = groupService;
    }

    public async Task<Result<string>> AddFileAsync(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var uploadResult =
                await _dropboxClient.Files.UploadAsync("/" + file.FileName, WriteMode.Overwrite.Instance, body: stream);

            var sharedLink = await _dropboxClient.Sharing.CreateSharedLinkWithSettingsAsync(uploadResult.PathDisplay);

            string link = sharedLink.Url;

            if (file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" ||
                file.ContentType == "application/msword")
            {
                return new Result<string>(true, link);
            }
            else
            {
                link = link.Replace("dl=0", "raw=1");

                return new Result<string>(true, link);
            }
        }
        catch (Exception ex)
        {
            return new Result<string>(false, $"File could not be loaded. ErrorMessage: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteFileAsync(string filePath)
    {
        try
        {
            await _dropboxClient.Files.DeleteV2Async("/" + filePath);

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"ErrorMessage: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddToGroup(IFormFile material, string url, int groupId)
    {
        var group = await _groupService.GetById(groupId);

        if (group == null)
        {
            return new Result<bool>(false, $"Failed to get {nameof(group)} by id {groupId}");
        }

        var materialFile = new EducationMaterial()
        {
            Name = material.FileName,
            Url = url,
            FileExtension = Path.GetExtension(material.FileName),
            MaterialAccess = MaterialAccess.Group,
            GroupId = group.Id,
            CourseId = group.CourseId
        };

        await _repository.AddAsync(materialFile);
        await _unitOfWork.Save();

        group.EducationMaterials.Add(materialFile);
        await _groupService.UpdateGroup(group);

        return new Result<bool>(true);
    }

    public async Task<Result<bool>> AddToCourse(IFormFile material, string url, int courseId)
    {
        var course = await _courseService.GetById(courseId);

        if (course == null)
        {
            return new Result<bool>(false, $"Failed to get {nameof(course)} by id {courseId}");
        }

        var materialFile = new EducationMaterial()
        {
            Name = material.FileName,
            Url = url,
            FileExtension = Path.GetExtension(material.FileName),
            MaterialAccess = MaterialAccess.Course,
            CourseId = course.Id,
            Course = course
        };

        await _repository.AddAsync(materialFile);
        await _unitOfWork.Save();

        course.EducationMaterials.Add(materialFile);
        await _courseService.UpdateCourse(course);

        return new Result<bool>(true);
    }

    public async Task<Result<bool>> DeleteFileFromGroup(EducationMaterial material)
    {
        var resultDeleteEducationMaterial = await DeleteFileAsync(material.Name);

        if (!resultDeleteEducationMaterial.IsSuccessful)
        {
            return new Result<bool>(false, $"Failed delete {nameof(material)}");
        }

        var resultDeleteFromDropBox = await DeleteUploadFileAsync(material);

        if (!resultDeleteFromDropBox.IsSuccessful)
        {
            return new Result<bool>(false, $"Failed delete {nameof(material)}");
        }

        var group = material.Group;

        if (group != null)
        {
            group.EducationMaterials.Remove(material);
            await _groupService.UpdateGroup(group);
        }

        return new Result<bool>(true);
    }

    public async Task<Result<List<EducationMaterial>>> GetAllMaterialAsync()
    {
        var materials = await _repository.GetAllAsync();

        if (!materials.Any())
        {
            return new Result<List<EducationMaterial>>(false, "Material list is empty");
        }

        return new Result<List<EducationMaterial>>(true, materials);
    }

    public async Task<Result<EducationMaterial>> GetByIdMaterialAsync(int id)
    {
        var material = await GetById(id);

        if (material == null)
        {
            return new Result<EducationMaterial>(false, $"{material} with id {id} does not exist");
        }

        return new Result<EducationMaterial>(true, material);
    }

    public async Task<Result<bool>> DeleteUploadFileAsync(EducationMaterial material)
    {
        try
        {
            await _repository.DeleteAsync(material);
            await _unitOfWork.Save();
            return new Result<bool>(true);
        }
        catch (Exception e)
        {
            return new Result<bool>(false, "Failed to delete material");
        }
    }
}