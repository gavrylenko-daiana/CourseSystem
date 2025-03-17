﻿using BLL.Interfaces;
using Core.Enums;
using Core.ImageStore;
using Core.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BLL.Services
{
    public class ProfileImageService : GenericService<ProfileImage>, IProfileImageService
    {
        private readonly IDropboxService _dropboxService;
        private readonly ILogger<ProfileImageService> _logger;

        public ProfileImageService(UnitOfWork unitOfWork, IDropboxService dropboxService, ILogger<ProfileImageService> logger) 
            : base(unitOfWork, unitOfWork.ProfileImageRepository)
        {
            _dropboxService = dropboxService;
            _logger = logger;
        }

        public Result<bool> CheckFileExtension(IFormFile newProfileImage)
        {
            var supportedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = System.IO.Path.GetExtension(newProfileImage.FileName);

            if (supportedImageExtensions.Contains(fileExtension.ToLower()))
            {
                _logger.LogInformation("Successfully {action} with file {entityName}",
                    MethodBase.GetCurrentMethod()?.Name, newProfileImage.Name);
                
                return new Result<bool>(true);
            }

            _logger.LogError("Failed to {action} with file {entityName}. An invalid file format!", 
                MethodBase.GetCurrentMethod()?.Name, newProfileImage.Name);
            
            return new Result<bool>(false, "An invalid file format has been sent. The file may have the extension .jpeg, .png, .jpg, .gif.");
        }

        public async Task<Result<bool>> DeleteUserProfileImage(AppUser user)
        {
            var userProfileImageResult = await GetByPredicate(p => p.AppUserId == user.Id);

            if (!userProfileImageResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Failed to get profile image - Message: {userProfileImageResult.Message}");
            }

            var defaultImages = await _dropboxService.GetAllFolderFilesData(DropboxFolders.DefaultProfileImages);
            var profileImage = userProfileImageResult.Data.FirstOrDefault();
            string profileImageName = profileImage.Name;
            string profileImageUrl = profileImage.Url;

            try
            {
                await _repository.DeleteAsync(profileImage);
                await _unitOfWork.Save();

                _logger.LogInformation("Successfully {action} for user {userId}",MethodBase.GetCurrentMethod()?.Name, user.Id);
                
                if (!DefaultProfileImage.IsProfileImageDefault(profileImageUrl, defaultImages.Data))
                {
                    var deleteImageDropboxResult = await _dropboxService.DeleteFileAsync(profileImageName, DropboxFolders.ProfileImages.ToString());

                    if (!deleteImageDropboxResult.IsSuccessful)
                    {
                        return new Result<bool>(false, $"Failed to delete profile image - Message: {deleteImageDropboxResult.Message}");
                    }
                }
                
                _logger.LogInformation("Successfully {action} for user {userId} and delete image from dropbox",
                    MethodBase.GetCurrentMethod()?.Name, user.Id);
               
                return new Result<bool>(true, "Successful deletion of the profile picture");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for user {userId} and delete image from dropbox. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, user.Id, ex.Message);
                
                return new Result<bool>(false, $"Failed to delete profile image - Message: {ex.Message}");
            }            
        }

        public async Task<Result<bool>> ReplaceUserProfileImage(AppUser user)
        {
            if (user == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(user));

                return new Result<bool>(false, "Invalid user");
            }

            var userProfileImageResult = await GetByPredicate(p => p.AppUserId == user.Id);

            if (!userProfileImageResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Failed to get profile image - Message: {userProfileImageResult.Message}");
            }

            var profileImage = userProfileImageResult.Data.FirstOrDefault();

            (string, string) newProfileImageData;

            var deletionResult = await DeleteCustomProfileImage(profileImage);

            if (deletionResult.IsSuccessful)
            {
                newProfileImageData = await GetAvatarsPack();
            }
            else
            {
                newProfileImageData = await GetAvatarsPack(profileImage?.Name);
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
                _logger.LogError("Failed to {action} for user {userId}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, user.Id, ex.Message);
                
                return new Result<bool>(false, $"Failed to replace profile image");
            }

            _logger.LogInformation("Successfully {action} for user {userId}",
                MethodBase.GetCurrentMethod()?.Name, user.Id);
            
            return new Result<bool>(true, "Successful replacement of the profile picture");
        }       

        public async Task<Result<bool>> SetDefaultProfileImage(AppUser user)
        {
            if (user == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(user));

                return new Result<bool>(false, "Invalid user");
            }
            
            var isNotDefaultImageResult = await IsNotDefaultProfileImage(user);

            if (!isNotDefaultImageResult.IsSuccessful)
            {
                return new Result<bool>(true);
            }

            var imageNameAndUrl = await GetAvatarsPack();

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

                _logger.LogInformation("Successfully {action} for user {userId}",
                    MethodBase.GetCurrentMethod()?.Name, user.Id);
                
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for user {userId}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, user.Id, ex.Message);
                
                return new Result<bool>(false, "Invalid user");
            }
        }

        public async Task<Result<bool>> UpdateProfileImage(AppUser user, IFormFile newProfileImage)
        {
            if (user == null || newProfileImage == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(user));

                return new Result<bool>(false, "Fail to update profile image");
            }

            try
            {
                var deleteResult = await DeleteCustomProfileImage(user.ProfileImage);

                if (!deleteResult.IsSuccessful)
                {
                    _logger.LogError("Failed to delete {entity} with id - {profileImageId} from dropbox", nameof(user.ProfileImage), user.ProfileImage.Id);
                }

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

                _logger.LogInformation("Successfully {action} for user {userId}",
                    MethodBase.GetCurrentMethod()?.Name, user.Id);
                
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for user {userId}. Error: {errorMsg}!", 
                    MethodBase.GetCurrentMethod()?.Name, user.Id, ex.Message);
                
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
                _logger.LogInformation("Successfully check {action} for user {userId}",
                    MethodBase.GetCurrentMethod()?.Name, user.Id);
                
                return new Result<bool>(true);
            }

            return new Result<bool>(false);
        }

        private async Task<(string, string)> GetAvatarsPack(string? exceptName = null)
        {
            (string, string) imageNameAndUrl;

            var dynemicAvatarsPackResult = await _dropboxService.GetAllFolderFilesData(DropboxFolders.DefaultProfileImages);

            if (!dynemicAvatarsPackResult.IsSuccessful)
            {
                imageNameAndUrl = DefaultProfileImage.GetDefaultImageUrl(exceptName);
            }
            else
            {
                imageNameAndUrl = DefaultProfileImage.GetDefaultImageUrl(exceptName, dynemicAvatarsPack: dynemicAvatarsPackResult.Data);
            }

            return imageNameAndUrl;
        }
        private async Task<Result<bool>> DeleteCustomProfileImage(ProfileImage profileImage)
        {
            if (profileImage == null)
            {
                return new Result<bool>(false, $"Invalid {nameof(profileImage)}");
            }

            var fileExist = await _dropboxService.FileExistsAsync(profileImage.Name, DropboxFolders.ProfileImages.ToString());

            if (fileExist.IsSuccessful)
            {
                var deleteImageDropboxResult = await _dropboxService.DeleteFileAsync(profileImage.Name, DropboxFolders.ProfileImages.ToString());

                if (!deleteImageDropboxResult.IsSuccessful)
                {
                    return new Result<bool>(false, $"Failed to delete profile image - Message: {deleteImageDropboxResult.Message}");
                }

                return new Result<bool>(true, $"Successful deletion of the {nameof(profileImage)}");
            }

            return new Result<bool>(false, $"{nameof(profileImage)} doesn't exist in folder");
        }
    }
}
