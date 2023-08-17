using Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IProfileImageService : IGenericService<ProfileImage>
    {
        Task<Result<bool>> SetDefaultProfileImage(AppUser user);
        Task<Result<bool>> UpdateProfileImage(AppUser user, IFormFile newProfileImage);
    }
}
