using Core.Models;

namespace BLL.Interfaces;

public interface IUserGroupService : IGenericService<UserGroups>
{
    Task<Result<bool>> CreateUserGroups(UserGroups userGroups);
    Task<Result<bool>> UpdateProgressInUserGroups(UserGroups userGroups, double progress);
    Task<bool> IsUserInGroup(UserGroups userGroups);
}