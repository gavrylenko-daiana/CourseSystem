using Core.Models;

namespace BLL.Interfaces;

public interface IGroupService : IGenericService<Group>
{
    Task CreateGroup(Group group, AppUser currentUser);
    Task DeleteGroup(int groupId);
    Task UpdateGroup(int groupId, string newName, DateTime startDate, DateTime endDate);
    Task<List<string>> GetAllStudentsEmailByIds(List<string> studentIds);
    Task<double> CalculateGroupProgress(int groupId);
}