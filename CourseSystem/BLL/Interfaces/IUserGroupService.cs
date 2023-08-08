using Core.Models;

namespace BLL.Interfaces;

public interface IUserGroupService : IGenericService<UserGroups>
{
    Task<Result<bool>> CreateUserGroups(UserGroups userGroups);
}