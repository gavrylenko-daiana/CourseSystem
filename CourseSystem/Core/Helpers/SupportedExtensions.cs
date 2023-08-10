namespace Core.Helpers;

public class SupportedExtensions
{
    public static string GetContentTypeByFileExtension(string fileExtension)
    {
        switch (fileExtension.ToLower())
        {
            case ".pdf": return "application/pdf";
            case ".jpg": return "image/jpg";
            case ".jpeg": return "image/jpeg";
            case ".png": return "image/png";
            case ".gif": return "image/gif";
            case ".bmp": return "image/bmp";
            case ".heic": return "image/heic";
            case ".mp4": return "video/mp4";
            case ".avi": return "video/avi";
            case ".mkv": return "video/x-matroska";
            case ".mov": return "video/quicktime";
            case ".doc": return "application/msword";
            case ".docx": return "application/msword";
            case ".zip": return "application/zip";
            case ".rar": return "application/x-rar-compressed";
            default: return null;
        }
    }
}