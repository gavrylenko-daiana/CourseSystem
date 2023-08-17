using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ProfileImageService : GenericService<ProfileImage>
    {
        protected ProfileImageService(UnitOfWork unitOfWork, IRepository<ProfileImage> repository) : base(unitOfWork, repository)
        {
        }
    }
}
