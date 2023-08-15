using Core.Models;

namespace BLL.Interfaces
{
    public interface IUserAssignmentService : IGenericService<UserAssignments>
    {
        Task<Result<UserAssignments>> CreateUserAssignment(Assignment assignment, AppUser appUser);
        Task<Result<bool>> ChangeUserAssignmentGrade(UserAssignments userAssignment, int newGrade);
        Task<Result<List<UserAssignments>>> GetAllUserAssignemnts(int assignmentId, string isMarked = null);
        Task<Result<UserAssignments>> GetUserAssignemnt(Assignment assignment, AppUser appUser);
    }
}
