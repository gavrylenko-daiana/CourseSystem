using Core.Models;

namespace BLL.Interfaces;

public interface IGroupService : IGenericService<Group>
{
    Task<Result<bool>> CreateGroup(Group group, AppUser currentUser);
    Task DeleteGroup(int groupId);
    Task<Result<bool>> UpdateGroup(Group newGroup);
    Task<List<string>> GetAllStudentsEmailByIds(List<string> studentIds);
    Task<double> CalculateGroupProgress(int groupId);
}