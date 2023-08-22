using Core.Enums;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAssignmentService : IGenericService<Assignment>
    {
        Task<Result<bool>> CreateAssignment(Assignment assignment);
        Task<Result<List<Assignment>>> GetGroupAssignments(int groupId, SortingParam sortOrder, string assignmentAccessFilter = null, string searchQuery = null);
        Task<Result<bool>> DeleteAssignment(int assignmentId);
        Task<Result<bool>> UpdateAssignment(Assignment assignment);
        Task<Result<bool>> ValidateTimeInput(DateTime? startDate, DateTime? endDate, int groupId);
    }
}
