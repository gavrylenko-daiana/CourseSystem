using System.Reflection;
using BLL.Interfaces;
using Core.Configuration;
using Core.Enums;
using Core.Models;
using DAL.Repository;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace BLL.Services;

public class DropboxService : IDropboxService
{
    private readonly DropboxClient _dropboxClient;
    private readonly ILogger<DropboxService> _logger;
    
    public DropboxService(IOptions<DropboxSettings> config, ILogger<DropboxService> logger)
    {
        _dropboxClient = new DropboxClient(config.Value.AccessToken);
        _logger = logger;
    }
    
    public async Task<Result<(string Url, string ModifiedFileName)>> AddFileAsync(IFormFile file, string? folder = null)
    {
        try
        {
            var uniquePathResult = await GetUniqueUploadPathAsync(file.FileName, folder);

            if (!uniquePathResult.IsSuccessful)
            {
                return new Result<(string Url, string ModifiedFileName)>(false, uniquePathResult.Message);
            }
            
            var modifiedFileName = uniquePathResult.Message;
            string uploadPath;

            if (folder == null)
            {
                uploadPath = "/" + modifiedFileName;
            }
            else
            {
                uploadPath = "/" + folder + "/" + modifiedFileName;
            }

            await using var stream = file.OpenReadStream();
            
            var uploadResult = await _dropboxClient.Files.UploadAsync(uploadPath, WriteMode.Overwrite.Instance, body: stream);
            var linkResult = await GetSharedLinkAsync(uploadResult.PathDisplay);

            if (!linkResult.IsSuccessful)
            {
                return new Result<(string Url, string ModifiedFileName)>(false, linkResult.Message);
            }

            var url = linkResult.Message.Replace("www.dropbox.com", "dl.dropboxusercontent.com");
            
            _logger.LogInformation("Successfully {action} with {entityName}", MethodBase.GetCurrentMethod()?.Name, file.Name);

            return new Result<(string Url, string ModifiedFileName)>(true, (url, modifiedFileName));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} with {entityName}. Error: {errorMsg}", 
                MethodBase.GetCurrentMethod()?.Name, file.Name, ex.Message);

            return new Result<(string Url, string ModifiedFileName)>(false, $"File could not be loaded. ErrorMessage - {ex.Message}");
        }
    }

    public async Task<Result<bool>> FileExistsInAnyFolderAsync(string filePath)
    {
        var tasks = new List<Task<Result<bool>>>();
    
        foreach (DropboxFolders folder in Enum.GetValues(typeof(DropboxFolders)))
        {
            tasks.Add(FileExistsAsync(filePath, folder.ToString()));
        }
    
        var results = await Task.WhenAll(tasks);
    
        foreach (var result in results)
        {
            if (result.IsSuccessful)
            {
                _logger.LogInformation("Successfully {action} with {filePath}", MethodBase.GetCurrentMethod()?.Name, filePath);

                return new Result<bool>(true);
            }
        }
    
        _logger.LogError("Failed to {action} with {filePath}", MethodBase.GetCurrentMethod()?.Name, filePath);
        
        return new Result<bool>(false, $"File '{filePath}' does not exist in any folder.");
    }

    public async Task<Result<bool>> DeleteFileAsync(string filePath, string? folder = null)
    {
        try
        {
            if (string.IsNullOrEmpty(folder))
            {
                await _dropboxClient.Files.DeleteV2Async("/" + filePath);
            }
            else
            {
                await _dropboxClient.Files.DeleteV2Async("/" + folder + "/" + filePath);
            }
            
            _logger.LogInformation("Successfully {action} with {filePath}", MethodBase.GetCurrentMethod()?.Name, filePath);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} with {filePath}", MethodBase.GetCurrentMethod()?.Name, filePath);
            
            return new Result<bool>(false, $"Failed to delete file. ErrorMessage - {ex.Message}");
        }
    }
    
    private async Task<Result<string>> GetUniqueUploadPathAsync(string fileName, string? folder = null)
    {
        int count = 1;
        var modifiedFileName = fileName;
        var resultExists = await FileExistsInAnyFolderAsync(fileName);

        while (resultExists.IsSuccessful)
        {
            var numberedFileNameResult = GetNumberedFileName(fileName, count);

            if (!numberedFileNameResult.IsSuccessful)
            {
                return new Result<string>(false, $"Failed to get unique {nameof(fileName)}. ErrorMessage - {numberedFileNameResult.Message}");
            }

            modifiedFileName = numberedFileNameResult.Message;
            count++;
            resultExists = await FileExistsInAnyFolderAsync(modifiedFileName);
        }
        
        _logger.LogInformation("Successfully {action} with {entityName}", MethodBase.GetCurrentMethod()?.Name, fileName);

        return new Result<string>(true, modifiedFileName);
    }
    
    private async Task<Result<string>> GetSharedLinkAsync(string pathDisplay)
    {
        try
        {
            var sharedLink = await _dropboxClient.Sharing.CreateSharedLinkWithSettingsAsync(pathDisplay);
            
            _logger.LogInformation("Successfully {action} with url {pathDisplay}", MethodBase.GetCurrentMethod()?.Name, pathDisplay);

            return new Result<string>(true, sharedLink.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to {action} with url {pathDisplay}. Error: {errorMsg}", 
                MethodBase.GetCurrentMethod()?.Name, pathDisplay, ex.Message);
            
            return new Result<string>(false, $"Failed to get url {nameof(pathDisplay)}. ErrorMessage - {ex.Message}");
        }
    }
    
    private Result<string> GetNumberedFileName(string fileName, int count)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            _logger.LogError("Failed to {action}. {entityName} is null or white space", 
                MethodBase.GetCurrentMethod()?.Name, fileNameWithoutExtension);

            return new Result<string>(false, $"The {nameof(fileNameWithoutExtension)} does not exist");
        }

        var fileExtension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            _logger.LogError("Failed to {action}. {entityName} is null or white space", 
                MethodBase.GetCurrentMethod()?.Name, fileExtension);
            
            return new Result<string>(false, $"The {nameof(fileExtension)} does not exist");
        }

        _logger.LogInformation("Successfully {action} with {entityName}", MethodBase.GetCurrentMethod()?.Name, fileName);

        return new Result<string>(true, $"{fileNameWithoutExtension}-{count}{fileExtension}");
    }
    
    private async Task<Result<bool>> FileExistsAsync(string filePath, string? folder = null)
    {
        try
        {
            if (string.IsNullOrEmpty(folder))
            {
                await _dropboxClient.Files.GetMetadataAsync("/" + filePath);
            }
            else
            {
                await _dropboxClient.Files.GetMetadataAsync("/" + folder + "/" + filePath);
            }
            
            _logger.LogInformation("Successfully check {action} with {filePath}", MethodBase.GetCurrentMethod()?.Name, filePath);
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to check {action} with {filePath}. Error: {errorMsg}", 
                MethodBase.GetCurrentMethod()?.Name, filePath, ex.Message);
            
            return new Result<bool>(false, $"Failed to get metadata by {nameof(filePath)}");
        }
    }
}