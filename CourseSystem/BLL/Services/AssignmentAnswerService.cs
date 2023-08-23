using System.Reflection;
using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class AssignmentAnswerService : GenericService<AssignmentAnswer>, IAssignmentAnswerService
    {
        private readonly IUserAssignmentService _userAssignmentService;
        private readonly IDropboxService _dropboxService;
        private readonly ILogger<AssignmentAnswerService> _logger;

        public AssignmentAnswerService(UnitOfWork unitOfWork, IUserAssignmentService userAssignmentService, IDropboxService dropboxService, ILogger<AssignmentAnswerService> logger)
            : base(unitOfWork, unitOfWork.AssignmentAnswerRepository)
        {
            _userAssignmentService = userAssignmentService;
            _dropboxService = dropboxService;
            _logger = logger;
        }

        public async Task<Result<bool>> CreateAssignmentAnswer(AssignmentAnswer assignmentAnswer, Assignment assignment,
            AppUser appUser, IFormFile? file, DropboxFolders folder)
        {
            if (assignmentAnswer == null)
            {
                _logger.LogError("Failed to {action} - {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(assignmentAnswer));

                return new Result<bool>(false, "Invalid assignment answer");
            }

            _logger.LogInformation("Forming {action} for user {userId} about assignmentAnswer {assignmentId}",
                MethodBase.GetCurrentMethod()?.Name, appUser.Id, assignment.Id);
            
            var userAssignmentResult = await _userAssignmentService.CreateUserAssignment(assignment, appUser);

            if (!userAssignmentResult.IsSuccessful)
            {
                _logger.LogError("Failed to {action} - Error: {errorMsg}!", MethodBase.GetCurrentMethod()?.Name, userAssignmentResult.Message);

                return new Result<bool>(false, "Failed to create user assignment");
            }

            if (file != null)
            {
                var fullPath = await _dropboxService.AddFileAsync(file, folder.ToString());

                if (!fullPath.IsSuccessful)
                {
                    return new Result<bool>(false, fullPath.Message);
                }
            
                assignmentAnswer.Name = fullPath.Data.ModifiedFileName;
                assignmentAnswer.Url = fullPath.Data.Url;
                assignmentAnswer.FileExtension = Path.GetExtension(fullPath.Data.ModifiedFileName);
            }
            else
            {
                assignmentAnswer.Name = string.Empty;
                assignmentAnswer.Url = string.Empty;
                assignmentAnswer.FileExtension = string.Empty;
            }
            
            assignmentAnswer.UserAssignment = userAssignmentResult.Data;
            assignmentAnswer.CreationTime = DateTime.Now;
            assignmentAnswer.DropboxFolder = folder;
            
            try
            {
                await _repository.AddAsync(assignmentAnswer);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} - Error: {errorMsg}!", MethodBase.GetCurrentMethod()?.Name, ex.Message);

                return new Result<bool>(false, ex.Message);
            }
        }

        public async Task<Result<bool>> DeleteAssignmentAnswer(AssignmentAnswer assignmentAnswer)
        {
            if (assignmentAnswer == null)
            {
                _logger.LogError("Failed to {action} - {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(assignmentAnswer));

                return new Result<bool>(false, "Fail to delete answer");
            }

            try
            {
                await _repository.DeleteAsync(assignmentAnswer);

                if (assignmentAnswer.UserAssignment.AssignmentAnswers.Count() == 1)
                {
                    await _unitOfWork.UserAssignmentsRepository.DeleteEntityByKeys(new object[] { assignmentAnswer.UserAssignment.AppUserId, assignmentAnswer.UserAssignment.AssignmentId });
                }

                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} - Error: {errorMsg}!", MethodBase.GetCurrentMethod()?.Name, ex.Message);

                return new Result<bool>(false, "Fail to delete answer");
            }
        }

        public async Task<Result<bool>> UpdateAssignmentAnswer(AssignmentAnswer assignmentAnswer)
        {
            if (assignmentAnswer == null)
            {
                _logger.LogError("Failed to {action} - {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(assignmentAnswer));

                return new Result<bool>(false, $"Invalid {nameof(assignmentAnswer)} data");
            }

            try
            {
                await _repository.UpdateAsync(assignmentAnswer);
                await _unitOfWork.Save();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to {action} - Error: {errorMsg}!", MethodBase.GetCurrentMethod()?.Name, ex.Message);

                return new Result<bool>(false, $"Fail to update {nameof(assignmentAnswer)}. Message - {ex.Message}");
            }
        }
    }
}