using System.Reflection;
using System.Security.Claims;
using System.Text;
using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Dropbox.Api.TeamLog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Core.Enums;
using MailKit.Search;
using static Dropbox.Api.Sharing.ListFileMembersIndividualResult;

namespace BLL.Services;

public class UserService : GenericService<AppUser>, IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IProfileImageService _profileImageService;
    private readonly ILogger<UserService> _logger;

    public UserService(UnitOfWork unitOfWork, UserManager<AppUser> userManager,
        IProfileImageService profileImageService, ILogger<UserService> logger)
        : base(unitOfWork, unitOfWork.UserRepository)
    {
        _userManager = userManager;
        _profileImageService = profileImageService;
        _logger = logger;
    }

    public async Task<Result<AppUser>> GetInfoUserByCurrentUserAsync(ClaimsPrincipal currentUser)
    {
        if (currentUser == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name,
                nameof(currentUser));

            return new Result<AppUser>(false, $"{nameof(currentUser)} does not exist");
        }

        var result = await GetCurrentUser(currentUser);

        if (result.IsSuccessful)
        {
            var user = result.Data;
            var appUser = await GetUserAfterMapping(user);

            return new Result<AppUser>(true, appUser);
        }
        else
        {
            return new Result<AppUser>(false, $"{nameof(currentUser)} does not exist");
        }
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

    public async Task<Result<bool>> EditUserAsync(ClaimsPrincipal currentUser, AppUser editUserViewModel,
        IFormFile? newProfileImage = null)
    {
        if (editUserViewModel == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name,
                nameof(editUserViewModel));

            return new Result<bool>(false, $"{nameof(editUserViewModel)} does not exist");
        }

        if (currentUser == null)
        {
            _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name,
                nameof(currentUser));

            return new Result<bool>(false, $"{nameof(currentUser)} does not exist");
        }

        var result = await GetCurrentUser(currentUser);

        if (result.IsSuccessful)
        {
            var user = result.Data;

            user.UserName = editUserViewModel.FirstName + editUserViewModel.LastName;
            user.FirstName = editUserViewModel.FirstName;
            user.LastName = editUserViewModel.LastName;
            user.BirthDate = editUserViewModel.BirthDate!;
            user.University = editUserViewModel.University!;
            user.Telegram = editUserViewModel.Telegram!;
            user.GitHub = editUserViewModel.GitHub!;
            user.Email = editUserViewModel.Email;

            if (newProfileImage != null)
            {
                var updateImageResult = await _profileImageService.UpdateProfileImage(user, newProfileImage);

                if (!updateImageResult.IsSuccessful)
                {
                    return new Result<bool>(false,
                        $"Failed to update {nameof(updateImageResult.Data)} - Message: {updateImageResult.Message}");
                }
            }

            await _userManager.UpdateAsync(user);

            return new Result<bool>(true);
        }
        else
        {
            return new Result<bool>(false, $"Failed to get {nameof(result.Data)} - Message: {result.Message}");
        }
    }

    public async Task<Result<bool>> CheckPasswordAsync(ClaimsPrincipal currentUser, string currentPassword,
        string newPassword)
    {
        var result = await GetCurrentUser(currentUser);

        if (result.IsSuccessful)
        {
            var user = result.Data;

            var checkPassword = await _userManager.CheckPasswordAsync(user, currentPassword);

            if (checkPassword)
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);

                await _userManager.UpdateAsync(user);

                return new Result<bool>(true);
            }
            else
            {
                return new Result<bool>(false, $"Failed to check password - Message: {result.Message}");
            }
        }
        else
        {
            return new Result<bool>(false, $"Message: {result.Message}");
        }
    }

    public async Task<Result<bool>> UpdatePasswordAsync(string email, string newPassword)
    {
        var userResult = await GetUserByEmailAsync(email);

        if (!userResult.IsSuccessful)
        {
            return new Result<bool>(false, userResult.Message);
        }

        userResult.Data.PasswordHash = _userManager.PasswordHasher.HashPassword(userResult.Data, newPassword);

        await _userManager.UpdateAsync(userResult.Data);

        _logger.LogInformation("Successfully to {action}.", MethodBase.GetCurrentMethod()?.Name);

        return new Result<bool>(true);
    }

    public async Task<Result<bool>> UpdateEmailAsync(string email, string newEmail)
    {
        var userResult = await GetUserByEmailAsync(email);

        if (!userResult.IsSuccessful)
        {
            return new Result<bool>(false, userResult.Message);
        }

        userResult.Data.Email = newEmail;

        await _userManager.UpdateAsync(userResult.Data);

        _logger.LogInformation("Successfully to {action}.", MethodBase.GetCurrentMethod()?.Name);

        return new Result<bool>(true);
    }

    public async Task<Result<AppUser>> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return new Result<AppUser>(false);
        }

        return new Result<AppUser>(true, user);
    }

    public async Task<Result<AppUser>> FindByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return new Result<AppUser>(false, $"{nameof(user)} does not exist");
        }

        return new Result<AppUser>(true, user);
    }

    public async Task<Result<AppUser>> GetCurrentUser(ClaimsPrincipal user)
    {
        var appUser = await _userManager.GetUserAsync(user);

        if (appUser == null)
        {
            return new Result<AppUser>(false, $"{nameof(appUser)} does not exist");
        }

        return new Result<AppUser>(true, appUser);
    }

    public string GenerateTemporaryPassword()
    {
        const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "1234567890";
        const string specialChars = "!@#$%^&*";

        var random = new Random();
        var password = new StringBuilder();

        password.Append(upperChars[random.Next(upperChars.Length)]);
        password.Append(lowerChars[random.Next(lowerChars.Length)]);
        password.Append(digitChars[random.Next(digitChars.Length)]);
        password.Append(specialChars[random.Next(specialChars.Length)]);

        var remainingChars = allowedChars + upperChars + lowerChars + digitChars + specialChars;

        for (int i = 0; i < 8; i++)
        {
            password.Append(remainingChars[random.Next(remainingChars.Length)]);
        }

        var shuffledPassword = new string(password.ToString().OrderBy(c => random.Next()).ToArray());

        return shuffledPassword;
    }

    public async Task<Result<List<AppUser>>> GetUsersAsync()
    {
        try
        {
            var users = await _userManager.Users.ToListAsync();

            return new Result<List<AppUser>>(true, users);
        }
        catch (Exception ex)
        {
            return new Result<List<AppUser>>(false, $"Failed to get all users. Message - {ex.Message}");
        }
    }

    public async Task<Result<IList<AppUser>>> GetUsersInRoleAsync(string role, string? searchQuery = null)
    {
        try
        {
            if (role == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name,
                    nameof(role));

                return new Result<IList<AppUser>>(false, $"{nameof(role)} was null");
            }

            var users = await _userManager.GetUsersInRoleAsync(role);

            if (searchQuery != null)
            {
                users = users.Where(s =>
                    s.FirstName.ToLower().Contains(searchQuery.ToLower()) ||
                    s.LastName.ToLower().Contains(searchQuery.ToLower())).ToList();
            }

            return new Result<IList<AppUser>>(true, users);
        }
        catch (Exception ex)
        {
            return new Result<IList<AppUser>>(false, $"Failed to get users in role {role}");
        }
    }
}