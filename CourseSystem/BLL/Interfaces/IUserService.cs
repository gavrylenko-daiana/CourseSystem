using System.Security.Claims;
using Core.Models;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<Result<AppUser>> GetInfoUserByCurrentUserAsync(ClaimsPrincipal user);
    
    Task<Result<AppUser>> GetInfoUserByIdAsync(string id);

    Task<Result<bool>> EditUserAsync(AppUser user, AppUser editUserViewModel);
}