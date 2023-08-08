using Core.Models;

namespace BLL.Interfaces
{
    public interface IAssignmentAnswerService
    {
        Task<Result<bool>> CreateAssignmentAnswer(AssignmentAnswer assignmentAnswer, Assignment assignment, AppUser appUser);
        Task<Result<bool>> DeleteAssignmentAnswer(AssignmentAnswer assignmentAnswer);
    }
}
