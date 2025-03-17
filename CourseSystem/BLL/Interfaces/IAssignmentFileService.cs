using BLL.Services;
using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IAssignmentFileService : IGenericService<AssignmentFile>
{
    Task<Result<AssignmentFile>> AddAssignmentFile(DropboxFolders folder, IFormFile file, int assignmentId);
    Task<Result<bool>> DeleteAssignmentFile(int fileId);
}