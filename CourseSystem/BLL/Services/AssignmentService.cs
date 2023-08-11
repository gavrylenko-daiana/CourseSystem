using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using DAL.Repository;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
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
                
            try
            {
                await _repository.DeleteAsync(assignment);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch(Exception ex)
            {
                return new Result<bool>(false, "Fail to delete assignment");
            }
        }

        public async Task<Result<List<Assignment>>> GetGroupAssignments(int groupId)
        {
            var group = await _groupService.GetById(groupId); 

            if (group == null)
            {
                return new Result<List<Assignment>>(false, "Fail to get group");
            }
                
            if (group.Assignments.IsNullOrEmpty())
            {
                return new Result<List<Assignment>>(true, "No assignment in group");
            }
                
            var groupAssignments = await ChechStartAndEndAssignmnetDate(group.Assignments);

            return new Result<List<Assignment>>(true, groupAssignments);
        }

        public Result<bool> ValidateTimeInput(DateTime? startDate, DateTime? endDate)
        {
            if(startDate == null)
            {
                startDate = DateTime.MinValue;
            }

            if(endDate == null)
            {
                endDate = DateTime.MaxValue;
            }

            if(startDate > endDate)
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

        private async Task<List<Assignment>> ChechStartAndEndAssignmnetDate(List<Assignment> assignments)
        {
            foreach(var assignment in assignments)
            {
                SetAssignmentStatus(assignment);

                await _repository.UpdateAsync(assignment);
            }

            await _unitOfWork.Save();

            return assignments;
        }

        private void SetAssignmentStatus(Assignment assignment)
        {
            if(assignment.StartDate > DateTime.Now)
            {
                assignment.AssignmentAccess = AssignmentAccess.Planned;
            }

            if(assignment.StartDate <= DateTime.Now)
            {
                assignment.AssignmentAccess = AssignmentAccess.InProgress;
            }

            if (assignment.EndDate < DateTime.Now)
            {
                assignment.AssignmentAccess = AssignmentAccess.Completed;
            }
        }
    }
}
