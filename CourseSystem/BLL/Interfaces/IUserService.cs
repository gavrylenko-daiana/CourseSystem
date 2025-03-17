using System.Security.Claims;
using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IUserService : IGenericService<AppUser>
{
    Task<Result<AppUser>> GetInfoUserByCurrentUserAsync(ClaimsPrincipal user);
    Task<Result<AppUser>> GetInfoUserByIdAsync(string id);
    Task<Result<bool>> EditUserAsync(ClaimsPrincipal user, AppUser editUserViewModel, IFormFile? newProfileImage = null);
    Task<Result<bool>> CheckPasswordAsync(ClaimsPrincipal currentUser, string currentPassword, string newPassword);
    Task<Result<bool>> UpdatePasswordAsync(string email, string newPassword);
    Task<Result<bool>> UpdateEmailAsync(string email, string newEmail);
    Task<Result<AppUser>> GetUserByEmailAsync(string email);
    Task<Result<AppUser>> FindByIdAsync(string id);
    Task<Result<AppUser>> GetCurrentUser(ClaimsPrincipal user);
    Task<Result<List<AppUser>>> GetUsersAsync();
    Task<Result<IList<AppUser>>> GetUsersInRoleAsync(string role, string? searchQuary = null);
    string GenerateTemporaryPassword();
}