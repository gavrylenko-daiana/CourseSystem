using Dropbox.Api;
using Core.Models;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IEducationMaterialService
{
    Task<Result<string>> AddFileAsync(IFormFile file);
    
    Task<Result<bool>> DeleteFileAsync(string filePath);

    Task<Result<List<EducationMaterial>>> GetAllMaterialAsync();

    Task<Result<bool>> AddToGroup(IFormFile material, string url, int groupId);
    
    Task<Result<bool>> AddToCourse(IFormFile material, string url, int courseId);

    Task<Result<EducationMaterial>> GetByIdMaterialAsync(int id);

    Task<Result<bool>> DeleteUploadFileAsync(EducationMaterial material);

    Task<Result<bool>> DeleteFileFromGroup(EducationMaterial material);
}