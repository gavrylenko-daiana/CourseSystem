using Core.Models;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IEducationMaterialService
{
    Task<Result<string>> AddFileAsync(IFormFile file);
    
    Task<Result<bool>> DeleteFileAsync(string filePath);

    Task<Result<List<EducationMaterial>>> GetAllMaterialAsync();

    Task<Result<Group>> AddToGroup(IFormFile material, string url, Group group);
    
    Task<Result<bool>> AddToCourse(IFormFile material, string url, Course course);
 
    Task<Result<EducationMaterial>> GetByIdMaterialAsync(int id);

    Task<Result<bool>> DeleteUploadFileAsync(EducationMaterial material);

    Task<Result<Group>> DeleteFileFromGroup(EducationMaterial material);

    Task<Result<bool>> UpdateMaterial(EducationMaterial material);
}