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
    public class UserAssignmentService : GenericService<UserAssignments>, IUserAssignmentService
    {
        private IUserGroupService _userGroupService;
        public UserAssignmentService(UnitOfWork unitOfWork, IUserGroupService userGroupService)
            : base(unitOfWork, unitOfWork.UserAssignmentsRepository)
        {
            _userGroupService = userGroupService;
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

        public async Task<Result<bool>> CreateUserAssignemntsForAllUsersInGroup(Assignment assignment)
        {
            if (assignment == null)
            {
                return new Result<bool>(false, $"Invalid input {nameof(assignment)} data");
            }      

            var allUserGroupsResult = await _userGroupService.GetByPredicate(ug => ug.GroupId == assignment.GroupId);

            if (!allUserGroupsResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Fail to get {nameof(allUserGroupsResult.Data)}");
            }

            var allGroupUsers = allUserGroupsResult.Data.Select(ug => ug.AppUser).ToList();

            var tasks = new List<Task<Result<UserAssignments>>>();

            foreach (var user in allGroupUsers)
            {
                tasks.Add(CreateUserAssignment(assignment, user));
            }

            var results = await Task.WhenAll(tasks);

            var isSuccsseful = results.Where(r => r.IsSuccessful).ToList();

            if (tasks.Count != isSuccsseful.Count)
            {
                return new Result<bool>(false, $"Fail to create {nameof(isSuccsseful)}");
            }

            return new Result<bool>(true);
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

                if (checkUserAssignment.Count != 0)
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
                    userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId && a.AssignmentAnswers.Count != 0 && a.AppUser.Role == AppUserRoles.Student && a.IsChecked == true);
                }
                else
                {
                    userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId && a.AssignmentAnswers.Count != 0 && a.AppUser.Role == AppUserRoles.Student && a.IsChecked == false);
                }
            }
            else
            {
                userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId && a.AssignmentAnswers.Count != 0 && a.AppUser.Role == AppUserRoles.Student);
            }

            return userAssignmentsResult;
        }

        public async Task<Result<UserAssignments>> GetUserAssignemnt(Assignment assignment, AppUser appUser)
        {
            var userAssignemnt = assignment.UserAssignments.FirstOrDefault(ua => ua.AssignmentId == assignment.Id && ua.AppUserId == appUser.Id);

            if (userAssignemnt == null)
            {
                var userAssignmentResult = await CreateUserAssignment(assignment, appUser);

                if(!userAssignmentResult.IsSuccessful)
                {
                    return new Result<UserAssignments>(false, userAssignmentResult.Message);
                }

                userAssignemnt = userAssignmentResult.Data;
            }

            return new Result<UserAssignments>(true, userAssignemnt);
        }
    }
}