using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services;

public class GroupService : GenericService<Group>, IGroupService
{
    private readonly IUserGroupService _userGroupService;
    
    public GroupService(UnitOfWork unitOfWork, IUserGroupService userGroupService) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.GroupRepository;
        _userGroupService = userGroupService;
    }

    public async Task CreateGroup(Group group, AppUser currentUser)
    {
        try
        {
            ValidateGroupDates(group.StartDate, group.EndDate);
                
            await Add(group);
            await _unitOfWork.Save();
            
            var userGroup = new UserGroups()
            {
                Group = group,
                GroupId = group.Id,
                AppUser = currentUser,
                AppUserId = currentUser.Id
            };
            await _userGroupService.CreateUserGroups(userGroup);
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
    
    private void ValidateGroupDates(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be less than end date.");
        }
    }
}