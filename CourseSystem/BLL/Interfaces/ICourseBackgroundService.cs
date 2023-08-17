using Core.Enums;
using Core.Models;

namespace BLL.Interfaces;

public interface ICourseBackgroundService
{
    Task<Result<string>> GetRandomDefaultBackground();
    // Task<Result<bool>> AddBackgroundImage(string backgroundName, string url);
    // Task<Result<EducationMaterial>> GetByIdMaterialAsync(int id);
    // Task<Result<bool>> DeleteFile(EducationMaterial material);
    // Task<Result<List<EducationMaterial>>> GetMaterialsListFromIdsString(string materialIds);
}