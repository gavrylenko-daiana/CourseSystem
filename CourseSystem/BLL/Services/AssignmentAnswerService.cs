using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services
{
    public class AssignmentAnswerService : GenericService<AssignmentAnswer>, IAssignmentAnswerService
    {
        private readonly IUserAssignmentService _userAssignmentService;
        public AssignmentAnswerService(UnitOfWork unitOfWork, IUserAssignmentService userAssignmentService) : base(unitOfWork)
        {
            _repository = unitOfWork.AssignmentAnswerRepository;
            _userAssignmentService = userAssignmentService;
        }

        public async Task<Result<bool>> CreateAssignmentAnswer(AssignmentAnswer assignmentAnswer, Assignment assignment, AppUser appUser)
        {
            if (assignmentAnswer == null)
                return new Result<bool>(false, "Invalid assignmnet answer");

            var userAssignmnetResult = await _userAssignmentService.CreateUserAssignment(assignment, appUser);

            if (!userAssignmnetResult.IsSuccessful)
                return new Result<bool>(false, "Failt to create user assignment");
            
            try
            {
                assignmentAnswer.UserAssignment = userAssignmnetResult.Data;
                await _repository.AddAsync(assignmentAnswer);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, ex.Message);
            }
        }
    }
}
