using Core.Models;

namespace BLL.Interfaces;

public interface IGroupService : IGenericService<Group>
{
    Task<Result<bool>> CreateGroup(Group group, AppUser currentUser);
    Task DeleteGroup(int groupId);
    Task<Result<bool>> UpdateGroup(Group newGroup);
    Task<double> CalculateGroupProgress(int groupId);
}