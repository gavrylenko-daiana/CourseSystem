using Core.Models;

namespace BLL.Interfaces
{
    public interface IAssignmentAnswerService : IGenericService<AssignmentAnswer>
    {
        Task<Result<bool>> CreateAssignmentAnswer(Assignment assignment, AppUser student);
    }
}
