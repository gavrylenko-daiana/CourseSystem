using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Http;

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
}