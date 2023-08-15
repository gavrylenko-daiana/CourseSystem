using Core.Enums;
using Core.Models;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IEducationMaterialService
{
    Task<Result<string>> AddFileAsync(IFormFile file);
    Task<Result<List<EducationMaterial>>> GetAllMaterialByAccessAsync(MaterialAccess access);
    // Task<Result<Group>> AddToGroup(IFormFile material, string url, Group group);
    // Task<Result<bool>> AddToCourse(IFormFile material, string url, Course course);
    // Task<Result<Group>> AddToGeneral(IFormFile material, string url);
    Task<Result<bool>> AddEducationMaterial(DateTime uploadTime, IFormFile material, string url, MaterialAccess materialAccess,
        Group group = null!, Course course = null!);
    Task<Result<EducationMaterial>> GetByIdMaterialAsync(int id);
    Task<Result<Group>> DeleteFileFromGroup(EducationMaterial material);
}