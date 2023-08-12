using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services
{
    public class AssignmentAnswerService : GenericService<AssignmentAnswer>, IAssignmentAnswerService
    {
        private readonly IUserAssignmentService _userAssignmentService;
        public AssignmentAnswerService(UnitOfWork unitOfWork, IUserAssignmentService userAssignmentService) 
            : base(unitOfWork, unitOfWork.AssignmentAnswerRepository)
        {
            _userAssignmentService = userAssignmentService;
        }

        public async Task<Result<bool>> CreateAssignmentAnswer(AssignmentAnswer assignmentAnswer, Assignment assignment, AppUser appUser)
        {
            if (assignmentAnswer == null)
            {
                return new Result<bool>(false, "Invalid assignment answer");
            }              

            var userAssignmentResult = await _userAssignmentService.CreateUserAssignment(assignment, appUser);

            if (!userAssignmentResult.IsSuccessful)
            {
                return new Result<bool>(false, "Failed to create user assignment");
            }
                            
            try
            {
                assignmentAnswer.UserAssignment = userAssignmentResult.Data;
                await _repository.AddAsync(assignmentAnswer);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, ex.Message);
            }
        }

        public async Task<Result<bool>> DeleteAssignmentAnswer(AssignmentAnswer assignmentAnswer)
        {
            if (assignmentAnswer == null)
            {
                return new Result<bool>(false, "Fail to delete answer");
            }

            try
            {
                await _repository.DeleteAsync(assignmentAnswer);

                if (assignmentAnswer.UserAssignment.AssignmentAnswers.Count() == 1)
                {
                    await _unitOfWork.UserAssignmentsRepository.DeleteEntityByKeys(new object[]{ assignmentAnswer.UserAssignment.AppUserId, assignmentAnswer.UserAssignment.AssignmentId});                   
                }

                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to delete answer");
            }
        }
    }
}
