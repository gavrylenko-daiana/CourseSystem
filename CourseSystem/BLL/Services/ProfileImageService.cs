using BLL.Interfaces;
using Core.Configuration;
using Core.ImageStore;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dropbox.Api.TeamLog.AccessMethodLogInfo;

namespace BLL.Services
{
    public class ProfileImageService : GenericService<ProfileImage>, IProfileImageService
    {
        private readonly IDropboxService _dropboxService;
        public ProfileImageService(UnitOfWork unitOfWork, IRepository<ProfileImage> repository, IOptions<DropboxSettings> config) : base(unitOfWork, repository)
        {
            string accessTokenProfile = config.Value.AccessTokenProfile;
            _dropboxService = new DropboxService(accessTokenProfile);
        }

        public async Task<Result<AppUser>> SetDefaultProfileImage(AppUser user)
        {
            if(user == null)
            {
                return new Result<AppUser>(false, "Invalid user");
            }

            var isNotDefaultImageResult = await IsNotDefaultProfileImage(user);

            if (!isNotDefaultImageResult.IsSuccessful)
            {
                return new Result<AppUser>(true, user);
            }

            var imageNameAndUrl = DefaultProfileImage.GetDefaultImageUrl();
            var profileImage = new ProfileImage()
            {
                AppUser = user,
                Name = imageNameAndUrl.Item1,
                Url = imageNameAndUrl.Item2
            };

            user.ProfileImage = profileImage;

            return new Result<AppUser>(true, user);
        }

        private async Task<Result<bool>> IsNotDefaultProfileImage(AppUser user)
        {
            var profileImageResult = await GetByPredicate(p => p.AppUserId == user.Id);

            if (!profileImageResult.IsSuccessful)
            {
                return new Result<bool>(false, "Fail to get profile image");
            }

            if(profileImageResult.Data.Count > 0)
            {
                return new Result<bool>(false);
            }

            return new Result<bool>(true);
        }
    }
}
