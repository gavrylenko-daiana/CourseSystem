using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces
{
    public interface IAssignmentAnswerService : IGenericService<AssignmentAnswer>
    {
        Task<Result<bool>> CreateAssignmentAnswer(AssignmentAnswer assignmentAnswer, Assignment assignment, AppUser appUser, IFormFile? file, DropboxFolders folder);
        Task<Result<bool>> DeleteAssignmentAnswer(AssignmentAnswer assignmentAnswer);
        Task<Result<bool>> UpdateAssignmentAnswer(AssignmentAnswer assignmentAnswer);
    }
}
