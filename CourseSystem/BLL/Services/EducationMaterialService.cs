using System.Net;
using BLL.Interfaces;
using Core.Configuration;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using DAL.Repository;
using Dropbox.Api;
using Dropbox.Api.Files;
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
    
    public async Task<string> AddFileAsync(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var uploadResult = await _dropboxClient.Files.UploadAsync("/" + file.FileName, WriteMode.Overwrite.Instance, body: stream);

            var sharedLink = await _dropboxClient.Sharing.CreateSharedLinkWithSettingsAsync(uploadResult.PathDisplay);
            
            string link = sharedLink.Url;

            if (file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" ||
                file.ContentType == "application/msword")
            {
                return link;
            }
            else
            {
                link = link.Replace("dl=0", "raw=1");
                
                return link;
            }
        }
        catch (Exception e)
        {
            throw new Exception($"File could not be loaded. ErrorMessage: {e.Message}");
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            await _dropboxClient.Files.DeleteV2Async("/" + filePath);
        }
        catch (Exception e)
        {
            throw new Exception($"File could not be deleted. ErrorMessage: {e.Message}");
        }
    }

    public async Task AddToGroup(IFormFile material, int groupId, string url)
    {
        var group = await _groupService.GetById(groupId);

        if (group == null) throw new Exception("failed to get group by groupId");

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