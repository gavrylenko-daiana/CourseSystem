using Core.Enums;
using Core.Models;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IEducationMaterialService : IGenericService<EducationMaterial>
{
    Task<Result<List<EducationMaterial>>> GetAllMaterialByAccessAsync(MaterialAccess access, SortingParam sortOrder);
    Task<Result<bool>> AddEducationMaterial(DateTime uploadTime, string materialName, string url, MaterialAccess materialAccess,
        Group group = null!, Course course = null!);
    Task<Result<EducationMaterial>> GetByIdMaterialAsync(int id);
    Task<Result<bool>> DeleteFile(EducationMaterial material);
    Task<Result<List<EducationMaterial>>> GetMaterialsListFromIdsString(string materialIds, SortingParam sortOrder, string searchQuery = null!);
}