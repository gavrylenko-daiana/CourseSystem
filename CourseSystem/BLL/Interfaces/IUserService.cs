using System.Security.Claims;
using Core.Models;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<Result<AppUser>> GetInfoUserByCurrentUserAsync(ClaimsPrincipal user);
    
    Task<Result<AppUser>> GetInfoUserByIdAsync(string id);

    Task<Result<bool>> EditUserAsync(ClaimsPrincipal user, AppUser editUserViewModel);

    Task<Result<bool>> CheckPasswordAsync(ClaimsPrincipal currentUser, string currentPassword, string newPassword);

    Task<Result<AppUser>> FindByIdAsync(string id);

    Task<Result<AppUser>> GetCurrentUser(ClaimsPrincipal user);
}