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
    private readonly IEducationMaterialService _educationMaterial;

    public GroupService(UnitOfWork unitOfWork, IUserGroupService userGroupService,
        IEducationMaterialService educationMaterial)
        : base(unitOfWork, unitOfWork.GroupRepository)
    {
        _userGroupService = userGroupService;
        _educationMaterial = educationMaterial;
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

            var addAdminsResult = await AddAllAdminsAtGroup(group);

            if (!addAdminsResult.IsSuccessful)
            {
                return new Result<bool>(false, $"{addAdminsResult.Message}");
            }

            if (currentUser.Role == AppUserRoles.Teacher)
            {
                var userGroup = new UserGroups()
                {
                    Group = group,
                    AppUser = currentUser
                };

                var createUserGroupResult = await CreateUserGroup(userGroup);

                if (!createUserGroupResult.IsSuccessful)
                {
                    return new Result<bool>(false, $"{createUserGroupResult.Message}");
                }
            }

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to create group {group.Name}. Exception: {ex.Message}");
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
            if (group.EducationMaterials.Any())
            {
                var educationMaterialsCopy = group.EducationMaterials.ToList();

                foreach (var material in educationMaterialsCopy)
                {
                    await _educationMaterial.DeleteFileFromGroup(material);
                }
            }

            await _repository.DeleteAsync(group);
            await _unitOfWork.Save();

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to delete group by id {groupId}. Exception: {ex.Message}");
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
            return new Result<bool>(false, $"Failed to update group {newGroup.Id}. Exception: {ex.Message}");
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

    public async Task<Result<List<Group>>> GetAllGroupsAsync()
    {
        var groups = await _repository.GetAllAsync();

        if (!groups.Any())
        {
            return new Result<List<Group>>(false, "Group list is empty");
        }

        return new Result<List<Group>>(true, groups);
    }

    private async Task<Result<bool>> AddAllAdminsAtGroup(Group group)
    {
        if (group.Course.UserCourses.Any())
        {
            foreach (var userCourse in group.Course.UserCourses)
            {
                if (userCourse.AppUser.Role == AppUserRoles.Admin)
                {
                    var userGroup = new UserGroups()
                    {
                        Group = group,
                        AppUser = userCourse.AppUser
                    };

                    var createUserGroupResult = await CreateUserGroup(userGroup);

                    if (!createUserGroupResult.IsSuccessful)
                    {
                        return new Result<bool>(false, $"{createUserGroupResult.Message}");
                    }
                }
            }
        }

        return new Result<bool>(true);
    }

    private async Task<Result<bool>> CreateUserGroup(UserGroups userGroups)
    {
        var createUserGroupResult = await _userGroupService.CreateUserGroups(userGroups);

        if (!createUserGroupResult.IsSuccessful)
        {
            await _repository.DeleteAsync(userGroups.Group);
            await _unitOfWork.Save();

            return new Result<bool>(false, $"{createUserGroupResult.Message}");
        }

        return new Result<bool>(true);
    }
}