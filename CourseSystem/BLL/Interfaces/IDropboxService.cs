using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public interface IDropboxService
{
    Task<Result<(string Url, string ModifiedFileName)>> AddFileAsync(IFormFile file);
    Task<Result<bool>> DeleteFileAsync(string filePath);
}