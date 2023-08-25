using System.Reflection;
using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class AssignmentFileService : GenericService<AssignmentFile>, IAssignmentFileService
{
    private readonly IDropboxService _dropboxService;
    private readonly ILogger<AssignmentFileService> _logger;

    public AssignmentFileService(UnitOfWork unitOfWork, IDropboxService dropboxService, ILogger<AssignmentFileService> logger) : base(unitOfWork,
        unitOfWork.AssignmentFileRepository)
    {
        _dropboxService = dropboxService;
        _logger = logger;
    }

    public async Task<Result<AssignmentFile>> AddAssignmentFile(DropboxFolders folder, IFormFile file, int assignmentId)
    {
        try
        {
            var fullPath = await _dropboxService.AddFileAsync(file, folder.ToString());
            
            if (!fullPath.IsSuccessful)
            {
                _logger.LogError("Failed to {action}. Error: {errorMsg}!", MethodBase.GetCurrentMethod()?.Name, fullPath.Message);

                return new Result<AssignmentFile>(false, fullPath.Message);
            }

            var assignmentFile = new AssignmentFile()
            {
                Name = fullPath.Data.ModifiedFileName,
                Url = fullPath.Data.Url,
                FileExtension = Path.GetExtension(fullPath.Data.ModifiedFileName),
                AssignmentId = assignmentId,
                DropboxFolder = folder
            };

            await _repository.AddAsync(assignmentFile);
            await _unitOfWork.Save();

            _logger.LogInformation("Successfully {action} with {entityName} for assignment by id {entityId}",
                MethodBase.GetCurrentMethod()?.Name, file.Name, assignmentId);
            
            return new Result<AssignmentFile>(true, assignmentFile);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} by {entityId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, assignmentId, ex.Message);

            return new Result<AssignmentFile>(false, $"Failed to add assignment file to database. Message: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAssignmentFile(int fileId)
    {
        try
        {
            var fileResult = await GetById(fileId);

            if (!fileResult.IsSuccessful)
            {
                _logger.LogError("Failed to {action}. Error: {errorMsg}!", MethodBase.GetCurrentMethod()?.Name, fileResult.Message);

                return new Result<bool>(false, fileResult.Message);
            }

            var deleteFileDropboxResult = await _dropboxService.DeleteFileAsync(fileResult.Data.Name, fileResult.Data.DropboxFolder.ToString());

            if (!deleteFileDropboxResult.IsSuccessful)
            {
                _logger.LogError("Failed to {action}. Error: {errorMsg}!", MethodBase.GetCurrentMethod()?.Name, fileResult.Message);
                
                return new Result<bool>(false, deleteFileDropboxResult.Message);
            }

            await _repository.DeleteAsync(fileResult.Data);
            await _unitOfWork.Save();
            
            _logger.LogInformation("Successfully {action} by {entityId}",
                MethodBase.GetCurrentMethod()?.Name, fileId);

            return new Result<bool>(true, $"Successful deletion of {nameof(fileResult.Data)}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} by {entityId}. Error: {errorMsg}!", 
                MethodBase.GetCurrentMethod()?.Name, fileId, ex.Message);

            return new Result<bool>(false, $"Fail to delete assignment file");
        }
    }
}