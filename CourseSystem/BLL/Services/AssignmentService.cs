using BLL.Interfaces;
using Core.Models;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AssignmentService : GenericService<Assignment>, IAssignmentService
    {
        public AssignmentService(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.AssignmentRepository;
        }

        public async Task<Result<bool>> CreateAssignment(Assignment assignment)
        {
            if (assignment == null)
                return new Result<bool>(false, "Invalid assignment data");

            try
            {
                await _repository.AddAsync(assignment);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Fail to save assignment");
            }
        }
    }
}
