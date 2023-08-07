using Core.Models;

namespace BLL.Interfaces;

public interface IUserGroupService
{
    Task CreateUserGroups(UserGroups userGroups);
}