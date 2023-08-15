using BLL.Interfaces;
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
    public class UserAssignmentService : GenericService<UserAssignments>, IUserAssignmentService
    {
        public UserAssignmentService(UnitOfWork unitOfWork)
            : base(unitOfWork, unitOfWork.UserAssignmentsRepository)
        {
        }

        public async Task<Result<bool>> ChangeUserAssignmentGrade(UserAssignments userAssignment, int newGrade)
        {
            if (userAssignment == null)
            {
                return new Result<bool>(false, $"Fail to get {nameof(userAssignment)}");
            }

            try
            {
                userAssignment.Grade = newGrade;
                userAssignment.IsChecked = true;

                await _repository.UpdateAsync(userAssignment);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to update assignment");
            }
        }

        public async Task<Result<UserAssignments>> CreateUserAssignment(Assignment assignment, AppUser appUser)
        {
            if (assignment == null || appUser == null)
            {
                return new Result<UserAssignments>(false, $"Invalid input {nameof(assignment)} and {nameof(appUser)} data");
            }

            try
            {
                var checkUserAssignment = await _repository.GetAsync(ua => ua.AppUserId == appUser.Id && ua.AssignmentId == assignment.Id);

                if (checkUserAssignment.Any())
                {
                    return new Result<UserAssignments>(true, checkUserAssignment.FirstOrDefault()!);
                }

                var userAssignment = new UserAssignments()
                {
                    Assignment = assignment,
                    AssignmentId = assignment.Id,
                    AppUser = appUser,
                    AppUserId = appUser.Id,
                };

                await _repository.AddAsync(userAssignment);
                await _unitOfWork.Save();

                return new Result<UserAssignments>(true, userAssignment);
            }
            catch (Exception ex)
            {
                return new Result<UserAssignments>(false, $"Fail to create {nameof(appUser)} {nameof(assignment)}");
            }
        }

        public async Task<Result<List<UserAssignments>>> GetAllUserAssignemnts(int assignmentId, string isMarked = null)
        {
            Result<List<UserAssignments>> userAssignmentsResult;

            if (!isMarked.IsNullOrEmpty())
            {
                if (isMarked.Equals("IsMarked"))
                {
                    userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId && a.IsChecked == true);
                }
                else
                {
                    userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId && a.IsChecked == false);
                }
            }
            else
            {
                userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId);
            }

            return userAssignmentsResult;
        }
    }
}