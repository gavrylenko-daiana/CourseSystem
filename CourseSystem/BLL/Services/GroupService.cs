using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services;

public class GroupService : GenericService<Group>, IGroupService
{
    public GroupService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.GroupRepository;
    }

    public async Task CreateGroup(Group group, AppUser currentUser)
    {
        try
        {
            await Add(group);
            await _unitOfWork.Save();
            
            var userGroup = new UserGroups()
            {
                Group = group,
                GroupId = group.Id,
                AppUser = currentUser,
                AppUserId = currentUser.Id
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create group {group.Name}. Exception: {ex.Message}");
        }
    }

    public async Task DeleteGroup(int groupId)
    {
        try
        {
            var group = await GetById(groupId);
            
            if (group == null)
            {
                throw new Exception("Course not found");
            }

            await Delete(group.Id);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete group by id {groupId}. Exception: {ex.Message}");
        }
    }

    public async Task UpdateGroup(int groupId)
    {
        try
        {
            var group = await GetById(groupId);
            
            if (group == null)
            {
                throw new Exception("Course not found");
            }
            
            await Update(group);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update group by id {groupId}. Exception: {ex.Message}");
        }
    }

    public async Task SentApprovalForAdmin(int groupId)
    {
        try
        {

        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to sent approval for admin in group by id {groupId}. Exception: {ex.Message}");
        }
    }
}