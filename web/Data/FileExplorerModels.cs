using System.ComponentModel.DataAnnotations;

namespace SftpCopyTool.Web.Data;

public class FileItem
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string FormattedSize => FormatFileSize(Size);
    public string Icon => IsDirectory ? "fas fa-folder" : GetFileIcon();

    private static string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 B";

        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return $"{size:N1} {suffixes[suffixIndex]}";
    }

    private string GetFileIcon()
    {
        var extension = Path.GetExtension(Name).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "fas fa-file-pdf text-danger",
            ".doc" or ".docx" => "fas fa-file-word text-primary",
            ".xls" or ".xlsx" => "fas fa-file-excel text-success",
            ".ppt" or ".pptx" => "fas fa-file-powerpoint text-warning",
            ".txt" or ".log" => "fas fa-file-alt",
            ".zip" or ".rar" or ".7z" => "fas fa-file-archive",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "fas fa-file-image text-info",
            ".mp4" or ".avi" or ".mkv" or ".mov" => "fas fa-file-video text-purple",
            ".mp3" or ".wav" or ".flac" => "fas fa-file-audio text-success",
            ".html" or ".htm" => "fas fa-file-code text-warning",
            ".css" => "fas fa-file-code text-info",
            ".js" => "fas fa-file-code text-warning",
            ".cs" => "fas fa-file-code text-purple",
            ".json" or ".xml" => "fas fa-file-code",
            _ => "fas fa-file"
        };
    }
}

public class DirectoryContents
{
    public string CurrentPath { get; set; } = string.Empty;
    public string? ParentPath { get; set; }
    public List<FileItem> Items { get; set; } = new();
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class PathBreadcrumb
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsLast { get; set; }
}
