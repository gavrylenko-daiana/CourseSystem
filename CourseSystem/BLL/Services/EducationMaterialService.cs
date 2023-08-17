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
    private readonly IDropboxService _dropboxService;

    public EducationMaterialService(UnitOfWork unitOfWork, IOptions<DropboxSettings> config) 
        : base(unitOfWork, unitOfWork.EducationMaterialRepository)
    {
        string accessTokenProfile = config.Value.AccessToken;
        _dropboxService = new DropboxService(accessTokenProfile);
    }

    public async Task<Result<bool>> AddEducationMaterial(DateTime uploadTime, string materialName, string url, MaterialAccess materialAccess, Group group = null!, Course course = null!)
    {
        try
        {
            var materialFile = new EducationMaterial()
            {
                Name = materialName,
                Url = url,
                FileExtension = Path.GetExtension(materialName),
                MaterialAccess = materialAccess,
                UploadTime = uploadTime
            };

            if (group != null)
            {
                materialFile.GroupId = group.Id;
                materialFile.CourseId = group.CourseId;

                await _repository.AddAsync(materialFile);
                await _unitOfWork.Save();
            }
            else if (course != null)
            {
                materialFile.CourseId = course.Id;
                materialFile.Course = course;

                await _repository.AddAsync(materialFile);
                await _unitOfWork.Save();
            }
            else
            {
                await _repository.AddAsync(materialFile);
                await _unitOfWork.Save();
            }
        
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"ErrorMessage - {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteFile(EducationMaterial material)
    {
        var resultDeleteEducationMaterial = await _dropboxService.DeleteFileAsync(material.Name);

        if (!resultDeleteEducationMaterial.IsSuccessful)
        {
            return new Result<bool>(false, $"Failed to delete {nameof(material)}");
        }

        var resultDeleteFromDropBox = await DeleteUploadFileAsync(material);

        if (!resultDeleteFromDropBox.IsSuccessful)
        {
            return new Result<bool>(false, $"Failed to delete {nameof(material)}");
        }

        return new Result<bool>(true);
    }

    public async Task<Result<List<EducationMaterial>>> GetAllMaterialByAccessAsync(MaterialAccess access)
    {
        var materials = await _repository.GetAllAsync();
        materials = materials.Where(e => e.MaterialAccess == access).ToList();

        if (!materials.Any())
        {
            return new Result<List<EducationMaterial>>(false, "Material list is empty");
        }

        return new Result<List<EducationMaterial>>(true, materials);
    }

    public async Task<Result<EducationMaterial>> GetByIdMaterialAsync(int id)
    {
        var materialResult = await GetById(id);

        if (!materialResult.IsSuccessful)
        {
            return new Result<EducationMaterial>(false,
                $"The {nameof(materialResult)} with id {id} does not exist. Message - {materialResult.Message}");
        }

        return new Result<EducationMaterial>(true, materialResult.Data);
    }

    private async Task<Result<bool>> DeleteUploadFileAsync(EducationMaterial material)
    {
        try
        {
            await _repository.DeleteAsync(material);
            await _unitOfWork.Save();

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to delete material. Message - {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateMaterial(EducationMaterial material)
    {
        try
        {
            await _repository.UpdateAsync(material);
            await _unitOfWork.Save();

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to update material by id {material.Id}. Exception: {ex.Message}");
        }
    }
    
    public async Task<Result<List<EducationMaterial>>> GetMaterialsListFromIdsString(string materialIds)
    {
        var materialsList = new List<EducationMaterial>();

        if (string.IsNullOrEmpty(materialIds))
        {
            return new Result<List<EducationMaterial>>(false, $"{nameof(materialsList)} is empty");
        }

        var idStrings = materialIds.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var idString in idStrings)
        {
            if (int.TryParse(idString, out int materialId))
            {
                var materialResult = await GetByIdMaterialAsync(materialId);

                if (materialResult.IsSuccessful)
                {
                    materialsList.Add(materialResult.Data);
                }
            }
        }

        return new Result<List<EducationMaterial>>(true, materialsList);
    }
}