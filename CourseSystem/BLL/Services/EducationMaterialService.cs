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
    }

    public async Task<List<EducationMaterial>> GetAllMaterialAsync()
    {
        var materials = await _repository.GetAllAsync();

        return materials;
    }

    public async Task<EducationMaterial> GetByIdMaterialAsync(int id)
    {
        var material = await GetById(id);

        return material;
    }

    public async Task DeleteUploadFileAsync(EducationMaterial material)
    {
        await _repository.DeleteAsync(material);
        await _unitOfWork.Save();
    }

}