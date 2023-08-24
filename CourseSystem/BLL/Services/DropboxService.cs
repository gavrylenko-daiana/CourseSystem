using BLL.Interfaces;
using Core.Configuration;
using Core.Enums;
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
            
            return new Result<bool>(true);
        }
        catch
        {
            return new Result<bool>(false, $"Failed to get metadata by {nameof(filePath)}");
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
                return new Result<bool>(true);
            }
        }
    
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
            
            return new Result<bool>(true);
        }
        catch (Exception ex)
        {
            return new Result<bool>(false, $"Failed to delete file. ErrorMessage - {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, string>>> GetAllFolderFilesData(DropboxFolders dropboxFolder)
    {      
        try
        {
            string folderPath = "/" + dropboxFolder.ToString();

            Dictionary<string, string> filesNameAndUrl = new();

            var folderItems = await _dropboxClient.Files.ListFolderAsync(folderPath);

            foreach (var item in folderItems.Entries)
            {
                if (item.IsFile)
                {
                    
                    var fileMetadata = (FileMetadata)item;
                    var existingLinks = await _dropboxClient.Sharing.ListSharedLinksAsync(fileMetadata.PathLower);
                    var existingLink = existingLinks.Links.FirstOrDefault();

                    string url = null;

                    if (existingLink == null)
                    {
                        var sharedLinkResult = await GetSharedLinkAsync(fileMetadata.PathDisplay);

                        if (!sharedLinkResult.IsSuccessful)
                        {
                            return new Result<Dictionary<string, string>>(false, sharedLinkResult.Message);
                        }

                        url = sharedLinkResult.Message;
                    }
                    else
                    {
                        url = existingLink.Url;
                    }

                    var tempUrl = url.Replace("www.dropbox.com", "dl.dropboxusercontent.com");

                    filesNameAndUrl.Add(item.Name, tempUrl);
                }
            }

            return new Result<Dictionary<string, string>>(true, filesNameAndUrl);
        }
        catch (Exception ex)
        {
            return new Result<Dictionary<string, string>>(false, "Error listing folder: " + ex.Message);
        }
    }
}