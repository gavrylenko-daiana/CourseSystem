using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services;

public class UserGroupService : GenericService<UserGroups>, IUserGroupService
{
    public UserGroupService(UnitOfWork unitOfWork) : base(unitOfWork, unitOfWork.UserGroupsRepository)
    {
    }

    public async Task CreateUserGroups(UserGroups userGroups)
    {
        try
        {
            await _repository.AddAsync(userGroups);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create userGroups {userGroups.Id}. Exception: {ex.Message}");
        }
    }
}