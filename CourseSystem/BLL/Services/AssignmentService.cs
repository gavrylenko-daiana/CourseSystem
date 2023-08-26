using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using DAL.Repository;
using MailKit.Search;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class AssignmentService : GenericService<Assignment>, IAssignmentService
    {
        private readonly IDropboxService _dropboxService;
        private readonly IUserAssignmentService _userAssignmentService;
        private readonly IGroupService _groupService;
        private readonly ILogger<AssignmentService> _logger;
        
        public AssignmentService(UnitOfWork unitOfWork, IDropboxService dropboxService, IUserAssignmentService userAssignmentService, 
            IGroupService groupService, ILogger<AssignmentService> logger)
            : base(unitOfWork, unitOfWork.AssignmentRepository)
        {
            _dropboxService = dropboxService;
            _userAssignmentService = userAssignmentService;
            _groupService = groupService;
            _logger = logger;
        }

        public async Task<Result<bool>> CreateAssignment(Assignment assignment)
        {
            if (assignment == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(assignment));

                return new Result<bool>(false, "Invalid assignment data");
            }

            SetAssignmentStatus(assignment);
                      
            try
            {
                await _repository.AddAsync(assignment);
                await _unitOfWork.Save();
                
                _logger.LogInformation("Successfully {action} with {entityName}",
                    MethodBase.GetCurrentMethod()?.Name, assignment.Name);

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} with {entityName}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, assignment.Name, ex.Message);
                
                return new Result<bool>(false, $"Fail to save assignment. Message - {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAssignment(int assignmentId)
        {
            var assignment = await _repository.GetByIdAsync(assignmentId);

            if (assignment == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(assignment));
                
                return new Result<bool>(false, "Fail to get assignment");
            }

            foreach (var notification in assignment.Notifications)
            {
                notification.AssignmentId = null;
                notification.GroupId = null;
                notification.CourseId = null;
            }

            foreach (var file in assignment.AssignmentFiles)
            {
                var resultDeleteEducationMaterial = await _dropboxService.DeleteFileAsync(file.Name, file.DropboxFolder.ToString());

                if (!resultDeleteEducationMaterial.IsSuccessful)
                {
                    _logger.LogError("Failed to {action}. Error: {errorMsg}!", 
                        MethodBase.GetCurrentMethod()?.Name, resultDeleteEducationMaterial.Message);
                    
                    return new Result<bool>(false, $"Failed to delete {nameof(assignment)}");
                }
            }

            try
            {
                await _repository.DeleteAsync(assignment);
                await _unitOfWork.Save();

                _logger.LogInformation("Successfully {action} by {entityId}",
                    MethodBase.GetCurrentMethod()?.Name, assignmentId);
                
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} by {entityId}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, assignmentId, ex.Message);
                
                return new Result<bool>(false, "Fail to delete assignment");
            }
        }

        public async Task<Result<List<Assignment>>> GetGroupAssignments(int groupId, SortingParam sortOrder, string assignmentAccessFilter = null, string searchQuery = null)
        {
            Result<List<Assignment>> assignmentResult = null;

            var query = GetOrderByExpression(sortOrder);

            if (!string.IsNullOrEmpty(searchQuery) && !string.IsNullOrEmpty(assignmentAccessFilter))
            {
                var tempFilter = Enum.Parse(typeof(AssignmentAccess), assignmentAccessFilter);
                assignmentResult = await GetByPredicate(a => a.GroupId == groupId && a.Name.Contains(searchQuery) && a.AssignmentAccess.Equals(tempFilter), query.Data);
            }
            else if (!string.IsNullOrEmpty(searchQuery))
            {
                assignmentResult = await GetByPredicate(a => a.GroupId == groupId && a.Name.Contains(searchQuery), query.Data);
            }
            else if(assignmentAccessFilter != null)
            {
                var tempFilter = Enum.Parse(typeof(AssignmentAccess), assignmentAccessFilter);
                assignmentResult = await GetByPredicate(a => a.GroupId == groupId && a.AssignmentAccess.Equals(tempFilter), query.Data);
            }
            else
            {
                assignmentResult = await GetByPredicate(a => a.GroupId == groupId, query.Data);
            }

            if (!assignmentResult.IsSuccessful)
            {
                _logger.LogError("Failed to {action}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, assignmentResult.Message);
                
                return new Result<List<Assignment>>(false, $"{assignmentResult.Message}");
            }

            if (assignmentResult.Data.IsNullOrEmpty())
            {
                _logger.LogInformation("Assignments are null or empty");
                
                return new Result<List<Assignment>>(true, "No assignment in group");
            }

            var groupAssignments = await CheckStartAndEndAssignmentDate(assignmentResult.Data);

            return new Result<List<Assignment>>(true, assignmentResult.Data);
        }

        public async Task<Result<bool>> ValidateTimeInput(DateTime? startDate, DateTime? endDate, int groupId)
        {
            startDate ??= DateTime.MinValue;
            endDate ??= DateTime.MaxValue;

            var group = await _groupService.GetById(groupId);

            if (!group.IsSuccessful)
            {
                _logger.LogError("Failed to {action}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, group.Message);
                
                return new Result<bool>(false, $"Group with id: {groupId} does not exists");
            }

            if (startDate > endDate)
            {
                _logger.LogError("Failed to {action} by {entityId}. Task start date cannot be greater than task end date.!", 
                    MethodBase.GetCurrentMethod()?.Name, groupId);
                
                return new Result<bool>(false, "Task start date cannot be greater than task end date.");
            }
            
            if (endDate > group.Data.EndDate)
            {
                _logger.LogError("Failed to {action} by {entityId}. Task end date cannot be greater than the end of the group!", 
                    MethodBase.GetCurrentMethod()?.Name, groupId);
                
                return new Result<bool>(false, "Task end date cannot be greater than the end of the group");
            }

            return new Result<bool>(true);
        }

        public async Task<Result<bool>> UpdateAssignment(Assignment assignment)
        {
            if (assignment == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(assignment));

                return new Result<bool>(false, "Invalid assignment data");
            }

            try
            {
                await _repository.UpdateAsync(assignment);
                await _unitOfWork.Save();

                _logger.LogInformation("Successfully {action} for {entityName}",
                    MethodBase.GetCurrentMethod()?.Name, assignment.Name);
                
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for {entityName}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, assignment.Name, ex.Message);
                
                return new Result<bool>(false, "Fail to update assignment");
            }
        }

        private async Task<List<Assignment>> CheckStartAndEndAssignmentDate(List<Assignment> assignments)
        {
            foreach (var assignment in assignments)
            {
                SetAssignmentStatus(assignment);

                await _repository.UpdateAsync(assignment);
            }

            await _unitOfWork.Save();

            _logger.LogInformation("Successfully {action} for {entity}",
                MethodBase.GetCurrentMethod()?.Name, nameof(assignments));
            
            return assignments;
        }

        private void SetAssignmentStatus(Assignment assignment)
        {
            if (assignment.StartDate > DateTime.Now)
            {
                assignment.AssignmentAccess = AssignmentAccess.Planned;
            }

            if (assignment.StartDate <= DateTime.Now)
            {
                assignment.AssignmentAccess = AssignmentAccess.InProgress;
            }

            if (assignment.EndDate < DateTime.Now)
            {
                assignment.AssignmentAccess = AssignmentAccess.Completed;
            }
            
            _logger.LogInformation("Successfully {action} for {entityName} with {assignmentAccess} access",
                MethodBase.GetCurrentMethod()?.Name, assignment.Name, assignment.AssignmentAccess);
        }

        private Result<Expression<Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>>> GetOrderByExpression(SortingParam sortBy)
        {

            Expression<Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>> query;

            switch (sortBy)
            {
                case SortingParam.NameDesc:
                    query = q => q.OrderByDescending(q => q.Name);
                    break;
                case SortingParam.StartDateDesc:
                    query = q => q.OrderByDescending(q => q.StartDate);
                    break;
                case SortingParam.StartDate:
                    query = q => q.OrderBy(q => q.StartDate);
                    break;
                case SortingParam.EndDate:
                    query = q => q.OrderBy(q => q.EndDate);
                    break;
                case SortingParam.EndDateDesc:
                    query = q => q.OrderByDescending(q => q.EndDate);
                    break;
                default:
                    query = q => q.OrderBy(q => q.Name);
                    break;
            }
            
            _logger.LogInformation("Successfully {action}, query: {query}",
                MethodBase.GetCurrentMethod()?.Name, query.ToString());

            return new Result<Expression<Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>>>(true, query);
        }

        public async Task<Result<List<Assignment>>> GetAllUserAssignemnts(AppUser appUser, SortingParam sortOrder, string assignmentAccessFilter = null)
        {
            if (appUser == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(appUser));

                return new Result<List<Assignment>>(false, $"Invalid {nameof(appUser)}");
            }

            var allGroupsIds = appUser.UserGroups.Where(ug => !ug.Group.GroupAccess.Equals(GroupAccess.Completed)).
                Select(ug => ug.GroupId); //or get this data with a help of db

            var allUserAssignments = new List<Assignment>();

            foreach(var groupId in allGroupsIds)
            {
                Result<List<Assignment>> assignmentResult = null;

                var query = GetOrderByExpression(sortOrder);

                if (!string.IsNullOrEmpty(assignmentAccessFilter))
                {
                    var tempFilter = Enum.Parse(typeof(AssignmentAccess), assignmentAccessFilter);
                    assignmentResult = await GetByPredicate(a => a.GroupId == groupId && a.AssignmentAccess.Equals(tempFilter), query.Data);
                }
                else
                {
                    assignmentResult = await GetByPredicate(a => a.GroupId == groupId, query.Data);
                }

                if (!assignmentResult.IsSuccessful)
                {
                    _logger.LogError("Failed to {action}. Error: {errorMsg}!", 
                        MethodBase.GetCurrentMethod()?.Name, assignmentResult.Message);
                    
                    return new Result<List<Assignment>>(false, assignmentResult.Message);
                }

                allUserAssignments.AddRange(assignmentResult.Data);
            }
            
            _logger.LogInformation("Successfully {action} by {sortParam} for user with id: {userId}",
                MethodBase.GetCurrentMethod()?.Name, sortOrder, appUser.Id);

            return new Result<List<Assignment>>(true, allUserAssignments);
        }
    }
}