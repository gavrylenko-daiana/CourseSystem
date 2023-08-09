using System.Security.Claims;
using Core.Models;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<Result<AppUser>> GetInfoUserByCurrentUser(ClaimsPrincipal user);
    
    
}