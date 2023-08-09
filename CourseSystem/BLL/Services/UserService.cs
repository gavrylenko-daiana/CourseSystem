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
    
    public UserService(UnitOfWork unitOfWork, UserManager<AppUser> userManager) : base(unitOfWork, unitOfWork.UserRepository)
    {
        _userManager = userManager;
    }
    
    public async Task<Result<AppUser>> GetInfoUserByCurrentUserAsync(ClaimsPrincipal user)
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
    
    public async Task<Result<AppUser>> GetInfoUserByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return new Result<AppUser>(false, $"{nameof(id)} does not exist");
        }
        
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return new Result<AppUser>(false, $"{nameof(user)} does not exist");
        }
        
        var appUser = await GetUserAfterMapping(user);

        return new Result<AppUser>(true, appUser);
    }

    private Task<AppUser> GetUserAfterMapping(AppUser currentUser)
    {
        var user = new AppUser();
        currentUser.MapTo(user);

        return Task.FromResult(user);
    }

    public async Task<Result<bool>> EditUserAsync(AppUser user, AppUser editUserViewModel)
    {
        if (user == null)
        {
            return new Result<bool>(false, $"{nameof(user)} does not exist");
        }
        
        if (editUserViewModel == null)
        {
            return new Result<bool>(false, $"{nameof(editUserViewModel)} does not exist");
        }
        
        user.UserName = editUserViewModel.FirstName + editUserViewModel.LastName;
        user.FirstName = editUserViewModel.FirstName;
        user.LastName = editUserViewModel.LastName;
        user.BirthDate = editUserViewModel.BirthDate!;
        user.University = editUserViewModel.University!;
        user.Telegram = editUserViewModel.Telegram!;
        user.GitHub = editUserViewModel.GitHub!;
        user.Email = editUserViewModel.Email;

        await _userManager.UpdateAsync(user);

        return new Result<bool>(true);
    }
}