using Core.Enums;
using Core.Models;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IEducationMaterialService
{
    Task<Result<(string Url, string ModifiedFileName)>> AddFileAsync(IFormFile file);
    Task<Result<List<EducationMaterial>>> GetAllMaterialByAccessAsync(MaterialAccess access);
    Task<Result<bool>> AddEducationMaterial(DateTime uploadTime, string materialName, string url, MaterialAccess materialAccess,
        Group group = null!, Course course = null!);
    Task<Result<EducationMaterial>> GetByIdMaterialAsync(int id);
    Task<Result<bool>> DeleteFile(EducationMaterial material);
    Task<List<EducationMaterial>> GetMaterialsListFromIdsString(string materialIds);
}