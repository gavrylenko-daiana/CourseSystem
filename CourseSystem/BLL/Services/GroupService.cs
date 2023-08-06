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
        UserManager<AppUser> userManager) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.GroupRepository;
        _userGroupService = userGroupService;
        _userManager = userManager;
    }

    public async Task CreateGroup(Group group, AppUser currentUser)
    {
        try
        {
            if (group == null)
            {
                throw new ArgumentNullException("Group is null");
            }

            if (currentUser == null)
            {
                throw new ArgumentNullException("User is null");
            }
            
            ValidateGroupDates(group.StartDate, group.EndDate);
                
            await Add(group);
            await _unitOfWork.Save();

            var userGroup = new UserGroups()
            {
                Group = group,
                AppUser = currentUser
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

    public async Task UpdateGroup(int groupId, string newName, DateTime startDate, DateTime endDate)
    {
        try
        {
            var group = await GetById(groupId);
            
            if (group == null)
            {
                throw new Exception("Course not found");
            }

            group.Name = newName;
            group.StartDate = startDate;
            group.EndDate = endDate;
            
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