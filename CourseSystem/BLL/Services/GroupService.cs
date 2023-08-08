using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services;

public class GroupService : GenericService<Group>, IGroupService
{
    private readonly IUserGroupService _userGroupService;
    private readonly UserManager<AppUser> _userManager;
    
    public GroupService(UnitOfWork unitOfWork, IUserGroupService userGroupService,
        UserManager<AppUser> userManager) 
            : base(unitOfWork, unitOfWork.GroupRepository)
    {
        _userGroupService = userGroupService;
        _userManager = userManager;
    }

    public async Task<Result<bool>> CreateGroup(Group group, AppUser currentUser)
    {
        if (group == null)
        {
            return new Result<bool>(false, $"{nameof(group)} not found");
        }
        
        if (currentUser == null)
        {
            return new Result<bool>(false, $"{nameof(currentUser)} not found");
        }
        
        if (group.StartDate > group.EndDate)
        {
            return new Result<bool>(false, $"Start date must be less than end date");
        }
        
        try
        {
            await _repository.AddAsync(group);
            await _unitOfWork.Save();

            var userGroup = new UserGroups()
            {
                Group = group,
                AppUser = currentUser
            };
            
            var createUserGroupResult = await _userGroupService.CreateUserGroups(userGroup);

            if (!createUserGroupResult.IsSuccessful)
            {
                await _repository.DeleteAsync(group);
                await _unitOfWork.Save();
            }

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,$"Failed to create group {group.Name}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteGroup(int groupId)
    {
        var group = await _repository.GetByIdAsync(groupId);
            
        if (group == null)
        {
            return new Result<bool>(false, $"Group by id {groupId} not found");
        }
        
        try
        {
            await _repository.DeleteAsync(group);
            await _unitOfWork.Save();
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,$"Failed to delete group by id {groupId}. Exception: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateGroup(Group newGroup)
    {
        if (newGroup == null)
        {
            return new Result<bool>(false, $"{nameof(newGroup)} not found");
        }
        
        if (newGroup.StartDate > newGroup.EndDate)
        {
            return new Result<bool>(false, $"Start date must be less than end date");
        }
        
        try
        {
            var group = await _repository.GetByIdAsync(newGroup.Id);
            group.Name = newGroup.Name;
            group.StartDate = newGroup.StartDate;
            group.EndDate = newGroup.EndDate;
            
            await _repository.UpdateAsync(group);
            await _unitOfWork.Save();
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false,$"Failed to update group {newGroup.Id}. Exception: {ex.Message}");
        }
    }
    
    public async Task<double> CalculateGroupProgress(int groupId)
    {
        var group = await _repository.GetByIdAsync(groupId);
        
        if (group == null || group.Assignments == null || group.Assignments.Count == 0)
        {
            return 0.0;
        }

        var totalAssignments = group.Assignments.Count;
        var completedAssignments = group.Assignments.Sum(a => a.UserAssignments.Count(ua => ua.Grade > 0));

        var groupProgress = (double)completedAssignments / (totalAssignments * group.UserGroups.Count) * 100;
        return groupProgress;
    }
}