using Core.Models;

namespace BLL.Interfaces;

public interface IGroupService : IGenericService<Group>
{
    Task CreateGroup(Group group, AppUser currentUser);
    Task DeleteGroup(int groupId);
    Task UpdateGroup(int groupId);
    
    Task SentApprovalForAdmin(int groupId);
}