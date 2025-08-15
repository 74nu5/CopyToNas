using SftpCopyTool.Web.Data;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace SftpCopyTool.Web.Services;

public interface IFileExplorerService
{
    Task<DirectoryContents> GetRemoteDirectoryAsync(string host, int port, string username, string password, string path);
    DirectoryContents GetLocalDirectory(string path);
    List<PathBreadcrumb> GetPathBreadcrumbs(string path, bool isRemote = false);
}

public class FileExplorerService : IFileExplorerService
{
    private readonly ILogger<FileExplorerService> _logger;

    public FileExplorerService(ILogger<FileExplorerService> logger)
    {
        _logger = logger;
    }

    public async Task<DirectoryContents> GetRemoteDirectoryAsync(string host, int port, string username, string password, string path)
    {
        var result = new DirectoryContents
        {
            CurrentPath = path
        };

        try
        {
            using var client = new SftpClient(host, port, username, password);
            await Task.Run(() => client.Connect());

            if (!client.IsConnected)
            {
                result.HasError = true;
                result.ErrorMessage = "Impossible de se connecter au serveur SFTP";
                return result;
            }

            // Normaliser le chemin
            var normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;
            if (!normalizedPath.StartsWith("/"))
                normalizedPath = "/" + normalizedPath;

            result.CurrentPath = normalizedPath;

            // Obtenir le chemin parent
            if (normalizedPath != "/")
            {
                var parentPath = Path.GetDirectoryName(normalizedPath.Replace('\\', '/'));
                if (string.IsNullOrEmpty(parentPath) || parentPath == ".")
                    parentPath = "/";
                result.ParentPath = parentPath;
            }

            // Lister le contenu du répertoire
            var items = await Task.Run(() => client.ListDirectory(normalizedPath));

            result.Items = items
                .Where(item => item.Name != "." && item.Name != "..")
                .Select(item => new FileItem
                {
                    Name = item.Name,
                    FullPath = item.FullName,
                    IsDirectory = item.IsDirectory,
                    Size = item.IsDirectory ? 0 : item.Length,
                    LastModified = item.LastWriteTime
                })
                .OrderByDescending(item => item.IsDirectory)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            client.Disconnect();
        }
        catch (Renci.SshNet.Common.SftpPathNotFoundException ex)
        {
            _logger.LogError(ex, "Chemin SFTP introuvable {Path}", path);
            result.HasError = true;
            result.ErrorMessage = $"Chemin introuvable: {ex.Message}";
        }
        catch (Renci.SshNet.Common.SshException ex)
        {
            _logger.LogError(ex, "Erreur SSH lors de la lecture du répertoire distant {Path}", path);
            result.HasError = true;
            result.ErrorMessage = $"Erreur de connexion SSH: {ex.Message}";
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Accès refusé au répertoire distant {Path}", path);
            result.HasError = true;
            result.ErrorMessage = $"Accès refusé: {ex.Message}";
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout lors de l'accès au répertoire distant {Path}", path);
            result.HasError = true;
            result.ErrorMessage = "Délai d'attente dépassé lors de la connexion";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la lecture du répertoire distant {Path}", path);
            result.HasError = true;
            result.ErrorMessage = $"Erreur: {ex.Message}";
            throw;
        }

        return result;
    }

    public DirectoryContents GetLocalDirectory(string path)
    {
        var result = new DirectoryContents
        {
            CurrentPath = path
        };

        try
        {
            // Normaliser le chemin
            var normalizedPath = string.IsNullOrWhiteSpace(path) ?
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) :
                Path.GetFullPath(path);

            result.CurrentPath = normalizedPath;

            // Obtenir le chemin parent
            var parentDirectory = Directory.GetParent(normalizedPath);
            if (parentDirectory != null)
            {
                result.ParentPath = parentDirectory.FullName;
            }

            // Obtenir les répertoires
            var directories = Directory.GetDirectories(normalizedPath)
                .Select(dir => new DirectoryInfo(dir))
                .Where(dir => !dir.Attributes.HasFlag(FileAttributes.Hidden) && !dir.Attributes.HasFlag(FileAttributes.System))
                .Select(dir => new FileItem
                {
                    Name = dir.Name,
                    FullPath = dir.FullName,
                    IsDirectory = true,
                    Size = 0,
                    LastModified = dir.LastWriteTime
                })
                .ToList();

            // Obtenir les fichiers
            var files = Directory.GetFiles(normalizedPath)
                .Select(file => new FileInfo(file))
                .Where(file => !file.Attributes.HasFlag(FileAttributes.Hidden) && !file.Attributes.HasFlag(FileAttributes.System))
                .Select(file => new FileItem
                {
                    Name = file.Name,
                    FullPath = file.FullName,
                    IsDirectory = false,
                    Size = file.Length,
                    LastModified = file.LastWriteTime
                })
                .ToList();

            // Combiner et trier
            result.Items = directories.Concat(files)
                .OrderByDescending(item => item.IsDirectory)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (UnauthorizedAccessException)
        {
            result.HasError = true;
            result.ErrorMessage = "Accès refusé à ce répertoire";
        }
        catch (DirectoryNotFoundException)
        {
            result.HasError = true;
            result.ErrorMessage = "Répertoire introuvable";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la lecture du répertoire local {Path}", path);
            result.HasError = true;
            result.ErrorMessage = $"Erreur: {ex.Message}";
            throw;
        }

        return result;
    }

    public List<PathBreadcrumb> GetPathBreadcrumbs(string path, bool isRemote = false)
    {
        var breadcrumbs = new List<PathBreadcrumb>();

        if (string.IsNullOrWhiteSpace(path))
        {
            return breadcrumbs;
        }

        if (isRemote)
        {
            // Chemin Unix/Linux
            var separator = '/';
            var parts = path.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            // Ajouter la racine
            breadcrumbs.Add(new PathBreadcrumb
            {
                Name = "🏠",
                Path = "/"
            });

            var currentPath = "";
            for (int i = 0; i < parts.Length; i++)
            {
                currentPath += "/" + parts[i];
                breadcrumbs.Add(new PathBreadcrumb
                {
                    Name = parts[i],
                    Path = currentPath
                });
            }
        }
        else
        {
            // Chemin Windows/Local
            var parts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                // Premier élément (lecteur sur Windows)
                var rootPath = parts[0];
                if (!rootPath.EndsWith(Path.VolumeSeparatorChar.ToString()))
                    rootPath += Path.DirectorySeparatorChar;

                breadcrumbs.Add(new PathBreadcrumb
                {
                    Name = rootPath,
                    Path = rootPath
                });

                var currentPath = rootPath;
                for (int i = 1; i < parts.Length; i++)
                {
                    currentPath = Path.Combine(currentPath, parts[i]);
                    breadcrumbs.Add(new PathBreadcrumb
                    {
                        Name = parts[i],
                        Path = currentPath
                    });
                }
            }
        }

        // Marquer le dernier élément
        if (breadcrumbs.Any())
        {
            breadcrumbs.Last().IsLast = true;
        }

        return breadcrumbs;
    }
}
