using BLL.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Configuration;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BLL.Services;

public class EducationMaterialService : GenericService<EducationMaterial>, IEducationMaterialService
{
    private readonly Cloudinary _cloudinary;
    private readonly ICourseService _courseService;
    private readonly IGroupService _groupService;

    public EducationMaterialService(UnitOfWork unitOfWork, IOptions<CloudinarySettings> config,
        ICourseService courseService, IGroupService groupService) : base(unitOfWork,
        unitOfWork.EducationMaterialRepository)
    {
        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(acc);
        _courseService = courseService;
        _groupService = groupService;
    }

    public async Task<UploadResult> AddFileAsync(IFormFile file)
    {
        try
        {
            var uploadResult = new RawUploadResult();

            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream)
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            return uploadResult;
        }
        catch (Exception e)
        {
            throw new Exception($"File could not be loaded. ErrorMessage: {e.Message}");
        }
    }

    public async Task<DeletionResult> DeleteFileAsync(string url)
    {
        try
        {
            var deleteParams = new DeletionParams(url);

            return await _cloudinary.DestroyAsync(deleteParams);
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