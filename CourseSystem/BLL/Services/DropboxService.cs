using BLL.Interfaces;
using Core.Configuration;
using Core.Models;
using DAL.Repository;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace BLL.Services;

public class DropboxService : IDropboxService
{
    private readonly DropboxClient _dropboxClient;

    public DropboxService(IOptions<DropboxSettings> config)
    {
        _dropboxClient = new DropboxClient(config.Value.AccessToken);
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

            if(folder == null)
            {
                uploadPath = "/" + modifiedFileName;
            }
            else
            {
                uploadPath = "/" + folder + "/" + modifiedFileName;
            }

            await using var stream = file.OpenReadStream();
            var uploadResult =
                await _dropboxClient.Files.UploadAsync(uploadPath, WriteMode.Overwrite.Instance, body: stream);

            var linkResult = await GetSharedLinkAsync(uploadResult.PathDisplay);

            if (!linkResult.IsSuccessful)
            {
                return new Result<(string Url, string ModifiedFileName)>(false, linkResult.Message);
            }

            var url = IsDocumentContentType(file.ContentType) ? linkResult.Message : linkResult.Message.Replace("dl=0", "raw=1");

            return new Result<(string Url, string ModifiedFileName)>(true, (url, modifiedFileName));
        }
        catch (Exception ex)
        {
            return new Result<(string Url, string ModifiedFileName)>(false, $"File could not be loaded. ErrorMessage - {ex.Message}");
        }
    }
    
    private async Task<Result<string>> GetUniqueUploadPathAsync(string fileName, string? folder = null)
    {
        int count = 1;
        var modifiedFileName = fileName;
        var resultExists = await FileExistsAsync(fileName, folder);

        while (resultExists.IsSuccessful)
        {
            var numberedFileNameResult = GetNumberedFileName(fileName, count);

            if (!numberedFileNameResult.IsSuccessful)
            {
                return new Result<string>(false, $"Failed to get unique {nameof(fileName)}. ErrorMessage - {numberedFileNameResult.Message}");
            }

            modifiedFileName = numberedFileNameResult.Message;
            count++;
            resultExists = await FileExistsAsync(modifiedFileName, folder);
        }

        return new Result<string>(true, modifiedFileName);
    }

    private async Task<Result<string>> GetSharedLinkAsync(string pathDisplay)
    {
        try
        {
            var sharedLink = await _dropboxClient.Sharing.CreateSharedLinkWithSettingsAsync(pathDisplay);

            return new Result<string>(true, sharedLink.Url);
        }
        catch (Exception ex)
        {
            return new Result<string>(false, $"Failed to get url {nameof(pathDisplay)}. ErrorMessage - {ex.Message}");
        }
    }

    private bool IsDocumentContentType(string contentType)
    {
        var isWord = contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" ||
                     contentType == "application/msword";

        return isWord;
    }
    
    private Result<string> GetNumberedFileName(string fileName, int count)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            return new Result<string>(false, $"The {nameof(fileNameWithoutExtension)} does not exist");
        }

        var fileExtension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            return new Result<string>(false, $"The {nameof(fileExtension)} does not exist");
        }

        return new Result<string>(true, $"{fileNameWithoutExtension}-{count}{fileExtension}");
    }

    public async Task<Result<bool>> FileExistsAsync(string filePath, string? folder = null)
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
            
            return new Result<bool>(true);
        }
        catch
        {
            return new Result<bool>(false, $"Failed to get metadata by {nameof(filePath)}");
        }
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
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to delete file. ErrorMessage - {ex.Message}");
        }
    }
}