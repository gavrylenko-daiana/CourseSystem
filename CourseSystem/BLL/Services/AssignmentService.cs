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
        protected AssignmentService(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.AssignmentRepository;
        }
    }
}
