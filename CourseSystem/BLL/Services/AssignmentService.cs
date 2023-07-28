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
        public AssignmentService(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.AssignmentRepository;
        }

        public async Task<Result<bool>> CreateAssignment(Assignment assignment)
        {
            if (assignment == null)
                return new Result<bool>(false, "Invalid assignment data");

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

        public async Task<Result<List<Assignment>>> GetGroupAssignments(int groupId)
        {
            var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId); // group service

            if (group == null)
                return new Result<List<Assignment>>(false, "Failt to get group");

            if (group.Assignments.IsNullOrEmpty())
                return new Result<List<Assignment>>(true, "No assignment in group");

            return new Result<List<Assignment>>(true, group.Assignments);

            //var groupAssignmentsBasedOnUserRole = new List<Assignment>();
            //var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

            //if (group == null)
            //    return new Result<List<Assignment>>(false, "Failt to get group");

            //if(group.Assignments.IsNullOrEmpty())
            //    return new Result<List<Assignment>>(true, "No assignment in group");

            //if (userRoles.Contains(AppUserRoles.Teacher.ToString()))
            //    return new Result<List<Assignment>>(true, group.Assignments);

            //if (userRoles.Contains(AppUserRoles.Student.ToString()))
            //    return new Result<List<Assignment>>(true, group.Assignments.Where(a => a.AssignmentAccess != AssignmentAccess.Planned).ToList());

            //return new Result<List<Assignment>>(true, group.Assignments);
        }
    }
}
