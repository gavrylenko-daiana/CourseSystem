using Core.Models;

namespace BLL.Interfaces
{
    public interface IUserAssignmentService : IGenericService<UserAssignments>
    {
        Task<Result<UserAssignments>> CreateUserAssignment(Assignment assignment, AppUser appUser);
        Task<Result<bool>> ChangeUserAssignmentGrade(UserAssignments userAssignment, int newGrade);
    }
}
