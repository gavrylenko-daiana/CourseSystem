﻿using BLL.Interfaces;
using Core.Configuration;
using Core.Enums;
using Core.ImageStore;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dropbox.Api.Sharing.ListFileMembersIndividualResult;
using static Dropbox.Api.TeamLog.AccessMethodLogInfo;

namespace BLL.Services
{
    public class ProfileImageService : GenericService<ProfileImage>, IProfileImageService
    {
        private readonly IDropboxService _dropboxService;
        public ProfileImageService(UnitOfWork unitOfWork, IRepository<ProfileImage> repository, IDropboxService dropboxService) : base(unitOfWork, repository)
        {
            _dropboxService = dropboxService;
        }

        public Result<bool> CheckFileExtension(IFormFile newProfileImage)
        {
            var supportedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = System.IO.Path.GetExtension(newProfileImage.FileName);

            if (supportedImageExtensions.Contains(fileExtension))
            {
                return new Result<bool>(true);
            }

            return new Result<bool>(false, "An invalid file format has been sent. The file may have the extension .jpeg, .png, .jpg, .gif.");
        }

        public async Task<Result<bool>> DeleteUserProfileImage(AppUser user)
        {
            var userProfileImageResult = await GetByPredicate(p => p.AppUserId == user.Id);

            if (!userProfileImageResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Failed to get profile image - Message: {userProfileImageResult.Message}");
            }

            var profileImage = userProfileImageResult.Data.FirstOrDefault();
            string profileImageName = profileImage.Name;
            string profileImageUrl = profileImage.Url;

            try
            {
                await _repository.DeleteAsync(profileImage);
                await _unitOfWork.Save();

                if (!DefaultProfileImage.IsProfileImageDefault(profileImageUrl))
                {
                    var deleteImageDropboxResult = await _dropboxService.DeleteFileAsync(profileImageName, DropboxFolders.ProfileImages.ToString());

                    if (!deleteImageDropboxResult.IsSuccessful)
                    {
                        return new Result<bool>(false, $"Failed to delete profile image - Message: {deleteImageDropboxResult.Message}");
                    }
                }
               
                return new Result<bool>(true, "Successful deletion of the profile picture");
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, $"Failed to delete profile image - Message: {ex.Message}");
            }            
        }

        public async Task<Result<bool>> ReplaceUserProfileImage(AppUser user)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            var userProfileImageResult = await GetByPredicate(p => p.AppUserId == user.Id);

            if (!userProfileImageResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Failed to get profile image - Message: {userProfileImageResult.Message}");
            }

            var profileImage = userProfileImageResult.Data.FirstOrDefault();

            (string, string) newProfileImageData;

            if (!DefaultProfileImage.IsProfileImageDefault(user.ProfileImage.Url))
            {
                var deleteImageDropboxResult = await _dropboxService.DeleteFileAsync(user.ProfileImage.Name, DropboxFolders.ProfileImages.ToString());

                if (!deleteImageDropboxResult.IsSuccessful)
                {
                    return new Result<bool>(false, $"Failed to delete profile image - Message: {deleteImageDropboxResult.Message}");
                }

                newProfileImageData = DefaultProfileImage.GetDefaultImageUrl();
            }
            else
            {
                newProfileImageData = DefaultProfileImage.GetDefaultImageUrl(profileImage?.Name);               
            }

            try
            {
                profileImage.Name = newProfileImageData.Item1;
                profileImage.Url = newProfileImageData.Item2;

                await _repository.UpdateAsync(profileImage);
                await _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, $"Failed to replace profile image");
            }

            return new Result<bool>(true, "Successful replacment of the profile picture");
        }

        public async Task<Result<bool>> SetDefaultProfileImage(AppUser user)
        {
            if (user == null)
            {
                return new Result<bool>(false, "Invalid user");
            }

            var isNotDefaultImageResult = await IsNotDefaultProfileImage(user);

            if (!isNotDefaultImageResult.IsSuccessful)
            {
                return new Result<bool>(true);
            }

            var imageNameAndUrl = DefaultProfileImage.GetDefaultImageUrl();
            var profileImage = new ProfileImage()
            {
                AppUser = user,
                Name = imageNameAndUrl.Item1,
                Url = imageNameAndUrl.Item2
            };

            try
            {
                await _repository.AddAsync(profileImage);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, "Invalid user");
            }

        }

        public async Task<Result<bool>> UpdateProfileImage(AppUser user, IFormFile newProfileImage)
        {
            if (user == null || newProfileImage == null)
            {
                return new Result<bool>(false, "Fail to update profile image");
            }

            try
            {
                var addDropboxResult = await _dropboxService.AddFileAsync(newProfileImage, DropboxFolders.ProfileImages.ToString());

                if (!addDropboxResult.IsSuccessful)
                {
                    return new Result<bool>(false, $"Failed to update {nameof(addDropboxResult.Data)} - Message: {addDropboxResult.Message}");
                }

                var currentProfileImageResult = await GetByPredicate(p => p.AppUserId == user.Id);

                if (!currentProfileImageResult.IsSuccessful)
                {
                    return new Result<bool>(false, currentProfileImageResult.Message);
                }

                var profileImage = currentProfileImageResult.Data.FirstOrDefault();

                profileImage.Url = addDropboxResult.Data.Url;
                profileImage.Name = addDropboxResult.Data.ModifiedFileName;

                await _repository.UpdateAsync(profileImage);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                return new Result<bool>(false, $"Failed to update profile image");
            }
        }

        private async Task<Result<bool>> IsNotDefaultProfileImage(AppUser user)
        {
            var profileImageResult = await GetByPredicate(p => p.AppUserId == user.Id);

            if (!profileImageResult.IsSuccessful)
            {
                return new Result<bool>(false, "Fail to get profile image");
            }

            if (profileImageResult.Data.Count == 0)
            {
                return new Result<bool>(true);
            }

            return new Result<bool>(false);
        }
    }
}
