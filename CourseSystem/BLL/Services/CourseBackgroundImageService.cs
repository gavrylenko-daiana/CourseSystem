using BLL.Interfaces;
using Core.Enums;
using Core.ImageStore;
using Core.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CourseBackgroundImageService : GenericService<CourseBackgroundImage>, ICourseBackgroundImageService
    {
        private readonly IDropboxService _dropboxService;
        private readonly ILogger<ProfileImageService> _logger;

        public CourseBackgroundImageService(UnitOfWork unitOfWork, IDropboxService dropboxService, ILogger<ProfileImageService> logger)
            : base(unitOfWork, unitOfWork.CourseBackgroundImageRepository)
        {
            _dropboxService = dropboxService;
            _logger = logger;
        }

        private bool CheckFileExtension(IFormFile newBackgroundImage)
        {
            var supportedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(newBackgroundImage.FileName);

            if (supportedImageExtensions.Contains(fileExtension.ToLower()))
            {
                return true;
            }

            _logger.LogError("Failed to {action} with file {entityName}. An invalid file format!",
                MethodBase.GetCurrentMethod()?.Name, newBackgroundImage.Name);

            return false;
        }

        public async Task<Result<bool>> DeleteCourseBackgroundImage(Course course)
        {
            if (course == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

                return new Result<bool>(false, "Fail to update profile image");
            }

            var CourseBackgroundImageResult = await GetByPredicate(bg => bg.CourseId == course.Id);

            if (!CourseBackgroundImageResult.IsSuccessful)
            {
                return new Result<bool>(false, $"Failed to get course background - Message: {CourseBackgroundImageResult.Message}");
            }

            var courseBackgroundImage = CourseBackgroundImageResult.Data.FirstOrDefault();

            if (!DefaultBackgroundImages.IsDefaultBackgroundImage(courseBackgroundImage.Url))
            {
                var deleteImageDropboxResult = await _dropboxService.DeleteFileAsync(courseBackgroundImage.Name, DropboxFolders.CourseBackgroundImages.ToString());

                if (!deleteImageDropboxResult.IsSuccessful)
                {
                    return new Result<bool>(false, $"Failed to delete course background image from dropbox! Message: {deleteImageDropboxResult.Message}");
                }
            }

            try
            {
                await _repository.DeleteAsync(courseBackgroundImage);
                await _unitOfWork.Save();

                _logger.LogInformation("Successfully {action} for course {courseId}", 
                    MethodBase.GetCurrentMethod()?.Name, course.Id);

                return new Result<bool>(true, "Course background successfully deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for course {courseId}! Error: {errorMsg}!",
                    MethodBase.GetCurrentMethod()?.Name, course.Id, ex.Message);

                return new Result<bool>(false, $"Failed to delete course background - Message: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateBackgroundImage(Course course, IFormFile newCourseBackground)
        {
            if (course == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

                return new Result<bool>(false, "Fail to update profile image");
            }

            if (newCourseBackground == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(newCourseBackground));

                return new Result<bool>(false, "Fail to update profile image");
            }

            if (!CheckFileExtension(newCourseBackground))
            {
                _logger.LogError("Failed to {action}, invalid file extension {extension}!", 
                    MethodBase.GetCurrentMethod()?.Name, Path.GetExtension(newCourseBackground.FileName));

                return new Result<bool>(false, "Fail to update course background - invalid file extension");
            }

            try
            {
                var addDropboxResult = await _dropboxService.AddFileAsync(newCourseBackground, DropboxFolders.CourseBackgroundImages.ToString());

                if (!addDropboxResult.IsSuccessful)
                {
                    return new Result<bool>(false, $"Failed to update course background - Message: {addDropboxResult.Message}");
                }

                var currentBackgroundImageResult = await GetByPredicate(p => p.CourseId == course.Id);

                if (!currentBackgroundImageResult.IsSuccessful)
                {
                    return new Result<bool>(false, currentBackgroundImageResult.Message);
                }

                var currentBackground = currentBackgroundImageResult.Data.FirstOrDefault();

                if (!DefaultBackgroundImages.IsDefaultBackgroundImage(currentBackground.Url))
                {
                    var deleteImageDropboxResult = await _dropboxService.DeleteFileAsync(currentBackground.Name, DropboxFolders.CourseBackgroundImages.ToString());

                    if (!deleteImageDropboxResult.IsSuccessful)
                    {
                        return new Result<bool>(false, $"Failed to delete profile image - Message: {deleteImageDropboxResult.Message}");
                    }
                }

                currentBackground.Url = addDropboxResult.Data.Url;
                currentBackground.Name = addDropboxResult.Data.ModifiedFileName;

                await _repository.UpdateAsync(currentBackground);
                await _unitOfWork.Save();

                _logger.LogInformation("Successfully {action} for course {courseId}",
                    MethodBase.GetCurrentMethod()?.Name, course.Id);

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for course {courseId}. Error: {errorMsg}!",
                    MethodBase.GetCurrentMethod()?.Name, course.Id, ex.Message);

                return new Result<bool>(false, $"Failed to update profile image");
            }
        }

        public async Task<Result<bool>> SetDefaultBackgroundImage(Course course)
        {
            if (course == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

                return new Result<bool>(false, "Invalid user");
            }

            var randomBackgroundImage = DefaultBackgroundImages.GetRandomDefaultImage();

            var courseBackgroundImage = new CourseBackgroundImage()
            {
                Course = course,
                Name = randomBackgroundImage.Item1,
                Url = randomBackgroundImage.Item2
            };

            course.BackgroundImage = courseBackgroundImage;

            try
            {
                await _repository.AddAsync(courseBackgroundImage);
                await _unitOfWork.Save();

                _logger.LogInformation("Successfully {action} for course {courseId}",
                    MethodBase.GetCurrentMethod()?.Name, course.Id);

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for course {courseId}. Error: {errorMsg}!",
                    MethodBase.GetCurrentMethod()?.Name, course.Id, ex.Message);

                return new Result<bool>(false, "Invalid user");
            }
        }

        public async Task<Result<bool>> SetCustomBackgroundImage(Course course, IFormFile courseBackground)
        {
            if (course == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(course));

                return new Result<bool>(false, "Invalid user");
            }

            if (courseBackground == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(courseBackground));

                return new Result<bool>(false, "Invalid user");
            }

            if (!CheckFileExtension(courseBackground))
            {
                _logger.LogError("Failed to {action}, invalid file extension {extension}!",
                    MethodBase.GetCurrentMethod()?.Name, Path.GetExtension(courseBackground.FileName));

                return new Result<bool>(false, "Fail to update course background - invalid file extension");
            }

            try
            {
                var addDropboxResult = await _dropboxService.AddFileAsync(courseBackground, DropboxFolders.CourseBackgroundImages.ToString());

                if (!addDropboxResult.IsSuccessful)
                {
                    return new Result<bool>(false, $"Failed to update course background - Message: {addDropboxResult.Message}");
                }

                var courseBackgroundImage = new CourseBackgroundImage()
                {
                    Course = course,
                    Name = addDropboxResult.Data.ModifiedFileName,
                    Url = addDropboxResult.Data.Url
                };

                course.BackgroundImage = courseBackgroundImage;

                await _repository.AddAsync(courseBackgroundImage);
                await _unitOfWork.Save();

                _logger.LogInformation("Successfully {action} for course {courseId}",
                    MethodBase.GetCurrentMethod()?.Name, course.Id);

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} for course {courseId}. Error: {errorMsg}!",
                    MethodBase.GetCurrentMethod()?.Name, course.Id, ex.Message);

                return new Result<bool>(false, "Invalid user");
            }
        }
    }
}
