using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IDropboxService
{
    Task<Result<(string Url, string ModifiedFileName)>> AddFileAsync(IFormFile file, string? folder = null);
    Task<Result<bool>> DeleteFileAsync(string filePath, string? folder = null);
    Task<Result<bool>> FileExistsInAnyFolderAsync(string filePath);
    Task<Result<bool>> FileExistsAsync(string filePath, string? folder = null);
    Task<Result<Dictionary<string, string>>> GetAllFolderFilesData(DropboxFolders dropboxFolder);
}