using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using DAL.Repository;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AssignmentService : GenericService<Assignment>, IAssignmentService
    {
        private readonly IGroupService _groupService;

        public AssignmentService(UnitOfWork unitOfWork, IGroupService groupService)
            : base(unitOfWork, unitOfWork.AssignmentRepository)
        {
            _groupService = groupService;
        }

        public async Task<Result<bool>> CreateAssignment(Assignment assignment)
        {
            if (assignment == null)
            {
                return new Result<bool>(false, "Invalid assignment data");
            }

            SetAssignmentStatus(assignment);

            try
            {
                await _repository.AddAsync(assignment);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to save assignment");
            }
        }

        public async Task<Result<bool>> DeleteAssignment(int assignmentId)
        {
            var assignment = await _repository.GetByIdAsync(assignmentId);

            if (assignment == null)
            {
                return new Result<bool>(false, "Fail to get assignment");
            }

            foreach (var notification in assignment.Notifications)
            {
                notification.AssignmentId = null;
                notification.GroupId = null;
                notification.CourseId = null;
            }

            try
            {
                await _repository.DeleteAsync(assignment);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
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
                return new Result<List<Assignment>>(false, $"{assignmentResult.Message}");
            }

            if (assignmentResult.Data.IsNullOrEmpty())
            {
                return new Result<List<Assignment>>(true, "No assignment in group");
            }

            var groupAssignments = await CheckStartAndEndAssignmentDate(assignmentResult.Data);

            return new Result<List<Assignment>>(true, assignmentResult.Data);
        }

        public Result<bool> ValidateTimeInput(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.MinValue;
            endDate ??= DateTime.MaxValue;

            if (startDate > endDate)
            {
                return new Result<bool>(false, "End date can't be less than start date");
            }

            return new Result<bool>(true);
        }

        public async Task<Result<bool>> UpdateAssignment(Assignment assignment)
        {
            if (assignment == null)
            {
                return new Result<bool>(false, "Invalid assignment data");
            }

            try
            {
                await _repository.UpdateAsync(assignment);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
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

            return new Result<Expression<Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>>>(true, query);
        }
    }
}