using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using DAL.Repository;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class UserAssignmentService : GenericService<UserAssignments>, IUserAssignmentService
    {
        private IUserGroupService _userGroupService;
        private readonly ILogger<UserAssignmentService> _logger;

        public UserAssignmentService(UnitOfWork unitOfWork, IUserGroupService userGroupService, ILogger<UserAssignmentService> logger)
            : base(unitOfWork, unitOfWork.UserAssignmentsRepository)
        {
            _userGroupService = userGroupService;
            _logger = logger;
        }

        public async Task<Result<bool>> ChangeUserAssignmentGrade(UserAssignments userAssignment, int newGrade)
        {
            if (userAssignment == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(userAssignment));

                return new Result<bool>(false, $"Fail to get {nameof(userAssignment)}");
            }

            try
            {
                userAssignment.Grade = newGrade;
                userAssignment.IsChecked = true;

                await _repository.UpdateAsync(userAssignment);
                await _unitOfWork.Save();

                _logger.LogInformation("Successfully {action} with new grade {newGrade} for {entity}",
                    MethodBase.GetCurrentMethod()?.Name, newGrade, nameof(userAssignment));
                
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} with new grade {newGrade} for {entity}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, newGrade, nameof(userAssignment), ex.Message);
                
                return new Result<bool>(false, "Fail to update assignment");
            }
        }
       
        public async Task<Result<UserAssignments>> CreateUserAssignment(Assignment assignment, AppUser appUser)
        {
            if (assignment == null || appUser == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(assignment));

                return new Result<UserAssignments>(false, $"Invalid input {nameof(assignment)} and {nameof(appUser)} data");
            }

            try
            {
                var checkUserAssignment = await _repository.
                    GetAsync(ua => ua.AppUserId == appUser.Id && ua.AssignmentId == assignment.Id);

                if (checkUserAssignment == null || checkUserAssignment.Count == 0)
                {
                    var userAssignment = new UserAssignments()
                    {
                        Assignment = assignment,
                        AssignmentId = assignment.Id,
                        AppUser = appUser,
                        AppUserId = appUser.Id,
                    };

                    await _repository.AddAsync(userAssignment);
                    await _unitOfWork.Save();

                    _logger.LogInformation("Successfully {action} with {assignmentName} for user by {userId}",
                        MethodBase.GetCurrentMethod()?.Name, assignment.Name, appUser.Id);
                    
                    return new Result<UserAssignments>(true, userAssignment);
                }
                else
                {
                    return new Result<UserAssignments>(true, checkUserAssignment.FirstOrDefault()!);
                }               
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} with {assignmentName} for user by {userId}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, assignment.Name, appUser.Id, ex.Message);
                
                return new Result<UserAssignments>(false, $"Fail to create {nameof(appUser)} {nameof(assignment)}");
            }
        }

        public async Task<Result<List<UserAssignments>>> GetAllUserAssignments(int assignmentId, string isMarked = null)
        {
            Result<List<UserAssignments>> userAssignmentsResult;

            if (!isMarked.IsNullOrEmpty())
            {
                if (isMarked.Equals("IsMarked"))
                {
                    userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId && a.AssignmentAnswers.Count != 0 && 
                                                                      a.AppUser.Role == AppUserRoles.Student && a.IsChecked == true);
                }
                else
                {
                    userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId && a.AssignmentAnswers.Count != 0 && 
                                                                      a.AppUser.Role == AppUserRoles.Student && a.IsChecked == false);
                }
            }
            else
            {
                userAssignmentsResult = await GetByPredicate(a => a.AssignmentId == assignmentId && a.AssignmentAnswers.Count != 0 && 
                                                                  a.AppUser.Role == AppUserRoles.Student);
            }

            return userAssignmentsResult;
        }

        public async Task<Result<UserAssignments>> GetUserAssignment(Assignment assignment, AppUser appUser)
        {
            var userAssignment = assignment.UserAssignments.FirstOrDefault(ua => ua.AssignmentId == assignment.Id 
                && ua.AppUserId == appUser.Id);

            if (userAssignment == null)
            {
                var userAssignmentResult = await CreateUserAssignment(assignment, appUser);

                if (!userAssignmentResult.IsSuccessful)
                {
                    return new Result<UserAssignments>(false, userAssignmentResult.Message);
                }

                userAssignment = userAssignmentResult.Data;
            }

            _logger.LogInformation("Successfully {action} with {assignmentName} for user by {userId}",
                MethodBase.GetCurrentMethod()?.Name, assignment.Name, appUser.Id);
            
            return new Result<UserAssignments>(true, userAssignment);
        }
    }
}