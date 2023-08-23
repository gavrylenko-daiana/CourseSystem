using System.Linq.Expressions;
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

    public EducationMaterialService(UnitOfWork unitOfWork, IDropboxService dropboxService) 
        : base(unitOfWork, unitOfWork.EducationMaterialRepository)
    {
        _dropboxService = dropboxService;
    }

    public async Task<Result<bool>> AddEducationMaterial(DateTime uploadTime, string materialName, string url, MaterialAccess materialAccess, Group group = null!, Course course = null!)
    {
        try
        {
            var allEducationMaterialResult = await GetByPredicate(m => m.Name == materialName || m.Url == url);

            if (!allEducationMaterialResult.IsSuccessful)
            {
                return new Result<bool>(false, $"ErrorMessage - {allEducationMaterialResult.Message}");
            }

            if (!allEducationMaterialResult.Data.Any())
            {
                var materialFile = new EducationMaterial()
                {
                    Name = materialName,
                    Url = url,
                    FileExtension = Path.GetExtension(materialName),
                    MaterialAccess = materialAccess,
                    UploadTime = uploadTime
                };

                if (group != null && materialAccess.Equals(MaterialAccess.Group))
                {
                    materialFile.GroupId = group.Id;
                    materialFile.CourseId = group.CourseId;
                }
                else if (course != null && materialAccess.Equals(MaterialAccess.Course))
                {
                    materialFile.CourseId = course.Id;
                    materialFile.Course = course;
                }

                await _repository.AddAsync(materialFile);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            else
            {
                return new Result<bool>(false, $"Education material {materialName} already exist");
            }
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"ErrorMessage - {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteFile(EducationMaterial material)
    {
        var resultDeleteEducationMaterial = await _dropboxService.DeleteFileAsync(material.Name, material.MaterialAccess.ToString());

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

    public async Task<Result<List<EducationMaterial>>> GetAllMaterialByAccessAsync(MaterialAccess access, SortingParam sortOrder)
    {
        var materials = await _repository.GetAsync();
        var query = GetOrderByExpression(sortOrder);

        if (!materials.Any())
        {
            return new Result<List<EducationMaterial>>(false, "Material list is empty");
        }

        materials = (await GetByPredicate(e => e.MaterialAccess == access, query.Data)).Data;

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
    
    public async Task<Result<List<EducationMaterial>>> GetMaterialsListFromIdsString(string materialIds, SortingParam sortOrder, string searchQuery = null!)
    {
        var materialsList = new List<EducationMaterial>();

        if (string.IsNullOrEmpty(materialIds))
        {
            return new Result<List<EducationMaterial>>(false, $"{nameof(materialsList)} is empty");
        }

        var query = GetOrderByExpression(sortOrder);
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

        if (!string.IsNullOrEmpty(searchQuery))
        {
            materialsList = (await GetByPredicate(m => materialsList.Contains(m) && m.Name.Contains(searchQuery), query.Data)).Data;
        }
        else
        {
            materialsList = (await GetByPredicate(m => materialsList.Contains(m), query.Data)).Data;
        }
        
        return new Result<List<EducationMaterial>>(true, materialsList);
    }
    
    private Result<Expression<Func<IQueryable<EducationMaterial>, IOrderedQueryable<EducationMaterial>>>> GetOrderByExpression(SortingParam sortBy)
    {
        Expression<Func<IQueryable<EducationMaterial>, IOrderedQueryable<EducationMaterial>>> query;

        switch (sortBy)
        {
            case SortingParam.UploadTimeDesc:
                query = q => q.OrderByDescending(q => q.UploadTime);
                break;
            default:
                query = q => q.OrderBy(q => q.UploadTime);
                break;
        }

        return new Result<Expression<Func<IQueryable<EducationMaterial>, IOrderedQueryable<EducationMaterial>>>>(true, query);
    }

    public async Task<Result<bool>> ApprovedEducationMaterial(string fileName, string fileUrl)
    {
        var educationMaterialsResult = await GetByPredicate(m => m.Url == fileUrl && m.Name == fileName);

        if (!educationMaterialsResult.IsSuccessful)
        {
            return new Result<bool>(false, "There is no such education material");
        }

        var isApproved = educationMaterialsResult.Data.Any();

        if (isApproved)
        {
            return new Result<bool>(false, "This education material is already approved");
        }

        return new Result<bool>(true);
    }
       
}