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

    //public DropboxService(IOptions<DropboxSettings> config)
    //{
    //    _dropboxClient = new DropboxClient(config.Value.AccessToken);
    //}
    public DropboxService(string accessToken)
    {
        _dropboxClient = new DropboxClient(accessToken);
    }

    public async Task<Result<(string Url, string ModifiedFileName)>> AddFileAsync(IFormFile file)
    {
        try
        {
            var uniquePathResult = await GetUniqueUploadPathAsync(file.FileName);

            if (!uniquePathResult.IsSuccessful)
            {
                return new Result<(string Url, string ModifiedFileName)>(false, uniquePathResult.Message);
            }
            
            var modifiedFileName = uniquePathResult.Message;
            var uploadPath = "/" + modifiedFileName;

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
    
    private async Task<Result<string>> GetUniqueUploadPathAsync(string fileName)
    {
        int count = 1;
        var modifiedFileName = fileName;
        var resultExists = await FileExistsAsync(fileName);

        while (resultExists.IsSuccessful)
        {
            var numberedFileNameResult = GetNumberedFileName(fileName, count);

            if (!numberedFileNameResult.IsSuccessful)
            {
                return new Result<string>(false, $"Failed to get unique {nameof(fileName)}. ErrorMessage - {numberedFileNameResult.Message}");
            }

            modifiedFileName = numberedFileNameResult.Message;
            count++;
            resultExists = await FileExistsAsync(modifiedFileName);
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

    private async Task<Result<bool>> FileExistsAsync(string filePath)
    {
        try
        {
            await _dropboxClient.Files.GetMetadataAsync("/" + filePath);

            return new Result<bool>(true);
        }
        catch
        {
            return new Result<bool>(false, $"Failed to get metadata by {nameof(filePath)}");
        }
    }

    public async Task<Result<bool>> DeleteFileAsync(string filePath)
    {
        try
        {
            await _dropboxClient.Files.DeleteV2Async("/" + filePath);

            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to delete file. ErrorMessage - {ex.Message}");
        }
    }
}