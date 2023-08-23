using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services;

public class UserGroupService : GenericService<UserGroups>, IUserGroupService
{
    public UserGroupService(UnitOfWork unitOfWork) : base(unitOfWork, unitOfWork.UserGroupsRepository)
    {
    }

    public async Task<Result<bool>> CreateUserGroups(UserGroups userGroups)
    {
        if (userGroups == null)
        {
            return new Result<bool>(false, $"{nameof(userGroups)} not found");
        }

        try
        {
            if (!await IsUserInGroup(userGroups))
            {
                await _repository.AddAsync(userGroups);
                await _unitOfWork.Save();
            }

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to create userGroups {userGroups.Id}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateProgressInUserGroups(UserGroups userGroups, double progress)
    {
        if (userGroups == null)
        {
            return new Result<bool>(false, $"{nameof(userGroups)} not found");
        }

        try
        {
            await _repository.UpdateAsync(userGroups);
            await _unitOfWork.Save();

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,
                $"Failed to update progress {progress} in userGroups with {userGroups.Id}. Exception: {ex.Message}");
        }
    }

    public async Task<bool> IsUserInGroup(UserGroups userGroups)
    {
        var userGroup = await GetUserGroups(userGroups.AppUserId, userGroups.GroupId);

        return userGroup.Data != null;
    }

    private async Task<Result<UserGroups>> GetUserGroups(string? userId, int groupId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new Result<UserGroups>(false, $"{nameof(userId)} not found");
        }

        if (groupId < 1)
        {
            return new Result<UserGroups>(false, $"{nameof(groupId)} not found");
        }

        try
        {
            var userGroups = await _repository.GetAsync();
            var getUserGroups = userGroups.FirstOrDefault(ug => ug.AppUserId == userId && ug.GroupId == groupId);

            return new Result<UserGroups>(true, getUserGroups!);
        }
        catch (Exception ex)
        {
            return new Result<UserGroups>(false, $"Message - {ex.Message}");
        }
    }
}