using CloudinaryDotNet.Actions;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IEducationMaterialService
{
    Task<UploadResult> AddFileAsync(IFormFile file);

    Task<DeletionResult> DeleteFileAsync(string publicId);

    Task<List<EducationMaterial>> GetAllMaterialAsync();

    Task AddToGroup(IFormFile material, int groupId, string url);

    Task<EducationMaterial> GetByIdMaterialAsync(int id);
}