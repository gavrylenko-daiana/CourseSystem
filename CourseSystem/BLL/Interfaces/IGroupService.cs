using Core.Enums;
using Core.Models;

namespace BLL.Interfaces;

public interface IGroupService : IGenericService<Group>
{
    Task<Result<bool>> CreateGroup(Group group, AppUser currentUser);
    Task<Result<bool>> DeleteGroup(int groupId);
    Task<Result<bool>> DeleteUserFromGroup(Group group, AppUser deletedUser);
    Task<Result<bool>> UpdateGroup(Group newGroup);
    Task<string> CalculateStudentProgressInGroup(Group group, AppUser currentUser);
    Task<string> CalculateGroupProgress(int groupId);
    Task<Result<List<Group>>> GetAllGroupsAsync();
    Task<Result<List<Group>>> GetUserGroups(AppUser currentUser, SortingParam sortOrder, string groupAccessFilter = null, string searchQuery = null);
}