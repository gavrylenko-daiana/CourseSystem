using BLL.Interfaces;
using Core.Models;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserAssignmentService : GenericService<UserAssignments>, IUserAssignmentService
    {
        public UserAssignmentService(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.UserAssignmentsRepository;
        }

        public async Task<Result<UserAssignments>> CreateUserAssignment(Assignment assignment, AppUser appUser)
        {
            if(assignment == null || appUser == null)
                return new Result<UserAssignments>(false, "Invalid input assignmnet and user data");

            try
            {
                var chechUserAssignmnet = await _repository.GetAsync(ua => ua.AppUserId == appUser.Id && ua.AssignmentId == assignment.Id);

                if (chechUserAssignmnet.Any())
                    return new Result<UserAssignments>(true, chechUserAssignmnet.FirstOrDefault());

                var userAssignmnet = new UserAssignments()
                {
                    AssignmentId = assignment.Id,
                    AppUserId = appUser.Id,
                };

                await _repository.AddAsync(userAssignmnet);
                await _unitOfWork.Save();

                return new Result<UserAssignments>(true, userAssignmnet);
            }
            catch(Exception ex)
            {
                return new Result<UserAssignments>(false, "Fail to create user assignmnet");
            }
        }
    }
}
