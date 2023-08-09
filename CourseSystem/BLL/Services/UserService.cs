using System.Security.Claims;
using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services;

public class UserService : GenericService<AppUser>, IUserService
{
    private readonly UserManager<AppUser> _userManager;
    
    protected UserService(UnitOfWork unitOfWork, UserManager<AppUser> userManager) : base(unitOfWork, unitOfWork.UserRepository)
    {
        _userManager = userManager;
    }
    
    public async Task<Result<AppUser>> GetInfoUserByCurrentUser(ClaimsPrincipal user)
    {
        if (user == null)
        {
            return new Result<AppUser>(false, $"{nameof(user)} does not exist");
        }
        
        var currentUser = await _userManager.GetUserAsync(user);

        if (currentUser == null)
        {
            return new Result<AppUser>(false, $"{nameof(currentUser)} does not exist");
        }

        var appUser = await GetUserAfterMapping(currentUser);

        return new Result<AppUser>(true, appUser);
    }

    private async Task<AppUser> GetUserAfterMapping(AppUser currentUser)
    {
        var user = new AppUser();
        currentUser.MapTo(user);

        return user;
    }

}