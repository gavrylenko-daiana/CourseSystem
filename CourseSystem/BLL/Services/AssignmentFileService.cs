using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BLL.Services;

public class AssignmentFileService : GenericService<AssignmentFile>, IAssignmentFileService
{
    private readonly IDropboxService _dropboxService;

    public AssignmentFileService(UnitOfWork unitOfWork, IDropboxService dropboxService) : base(unitOfWork,
        unitOfWork.AssignmentFileRepository)
    {
        _dropboxService = dropboxService;
    }

    public async Task<Result<AssignmentFile>> AddAssignmentFile(DropboxFolders folder, IFormFile file, int assignmentId)
    {
        try
        {
            var fullPath = await _dropboxService.AddFileAsync(file, folder.ToString());
            
            if (!fullPath.IsSuccessful)
            {
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

            return new Result<AssignmentFile>(true, assignmentFile);
        }
        catch (Exception ex)
        {
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
                return new Result<bool>(false, fileResult.Message);
            }

            var deleteFileDropboxResult = await _dropboxService.DeleteFileAsync(fileResult.Data.Name, fileResult.Data.DropboxFolder.ToString());

            if (!deleteFileDropboxResult.IsSuccessful)
            {
                return new Result<bool>(false, deleteFileDropboxResult.Message);
            }

            await _repository.DeleteAsync(fileResult.Data);
            await _unitOfWork.Save();

            return new Result<bool>(true, $"Successful deletion of {nameof(fileResult.Data)}");
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Fail to delete assignment file");
        }
    }
}