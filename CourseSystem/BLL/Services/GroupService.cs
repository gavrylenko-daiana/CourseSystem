using BLL.Interfaces;
using Core.Enums;
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

    public async Task DeleteGroup(int groupId)
    {
        try
        {
            var group = await _repository.GetByIdAsync(groupId);
            
            if (group == null)
            {
                throw new Exception("Course not found");
            }

            await _repository.DeleteAsync(group);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete group by id {groupId}. Exception: {ex.Message}");
        }
    }

    public async Task UpdateGroup(int groupId, string newName, DateTime startDate, DateTime endDate)
    {
        try
        {
            var group = await _repository.GetByIdAsync(groupId);
            
            if (group == null)
            {
                throw new Exception("Course not found");
            }

            group.Name = newName;
            group.StartDate = startDate;
            group.EndDate = endDate;
            
            await _repository.UpdateAsync(group);
            await _unitOfWork.Save();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update group by id {groupId}. Exception: {ex.Message}");
        }
    }

    public async Task<List<string>> GetAllStudentsEmailByIds(List<string> studentIds)
    {
        var emails = new List<string>();

        foreach (var studentId in studentIds)
        {
            try
            {
                var student = await _userManager.FindByIdAsync(studentId);
                emails.Add(student.Email);
            }
            catch(Exception ex)
            {
                throw new Exception("Failt to get students");
            }
        }

        return emails;
    }

    public async Task<double> CalculateGroupProgress(int groupId)
    {
        try
        {
            var group = await _repository.GetByIdAsync(groupId);
            if (group == null || group.Assignments == null || group.Assignments.Count == 0)
            {
                return 0.0;
            }

            var totalAssignments = group.Assignments.Count;
            var completedAssignments = group.Assignments.Sum(a => a.UserAssignments.Count(ua => ua.Grade > 0));

            return (double)completedAssignments / (totalAssignments * group.UserGroups.Count) * 100;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to calculate progress in group by id {groupId}. Exception: {ex.Message}");
        }
    }
}