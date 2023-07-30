using Core.Models;

namespace BLL.Interfaces
{
    public interface IAssignmentAnswerService : IGenericService<AssignmentAnswer>
    {
        Task<Result<bool>> CreateAssignmentAnswer(AssignmentAnswer assignmentAnswer, Assignment assignment, AppUser appUser);
    }
}
