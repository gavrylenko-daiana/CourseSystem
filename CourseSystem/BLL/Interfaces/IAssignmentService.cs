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
        Task<Result<List<Assignment>>> GetGroupAssignments(int groupId);
        Task<Result<bool>> DeleteAssignment(int assignmentId);
    }
}
