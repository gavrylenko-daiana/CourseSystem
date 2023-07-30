using BLL.Interfaces;
using Core.Models;
using DAL.Repository;

namespace BLL.Services
{
    public class AssignmentAnswerService : GenericService<AssignmentAnswer>, IAssignmentAnswerService
    {
        public AssignmentAnswerService(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.AssignmentAnswerRepository;
        }

        public async Task<Result<bool>> CreateAssignmentAnswer(Assignment assignment, AppUser student)
        {
            throw new NotImplementedException();
        }
    }
}
