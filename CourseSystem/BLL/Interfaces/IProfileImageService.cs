using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IProfileImageService : IGenericService<ProfileImage>
    {
        Task<Result<AppUser>> SetDefaultProfileImage(AppUser user);
    }
}
