using Dropbox.Api;
using Core.Models;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IEducationMaterialService
{
    Task<string> AddFileAsync(IFormFile file);

    Task DeleteFileAsync(string publicId);

    Task<List<EducationMaterial>> GetAllMaterialAsync();

    Task AddToGroup(IFormFile material, int groupId, string url);

    Task<EducationMaterial> GetByIdMaterialAsync(int id);

    Task DeleteUploadFileAsync(EducationMaterial material);
}