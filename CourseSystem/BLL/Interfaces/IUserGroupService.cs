using Core.Models;

namespace BLL.Interfaces;

public interface IUserGroupService : IGenericService<UserGroups>
{
    Task CreateUserGroups(UserGroups userGroups);
}