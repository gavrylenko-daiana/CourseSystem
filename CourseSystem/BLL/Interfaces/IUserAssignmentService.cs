using Core.Models;

namespace BLL.Interfaces
{
    public interface IUserAssignmentService
    {
        Task<Result<UserAssignments>> CreateUserAssignment(Assignment assignment, AppUser appUser);
        Task<Result<bool>> ChangeUserAssignmentGrade(UserAssignments userAssignment, int newGrade);
    }
}
