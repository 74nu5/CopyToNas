using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace SftpCopyTool;

public class SftpService
{
    private readonly ILogger<SftpService> _logger;

    public SftpService(ILogger<SftpService> logger)
    {
        _logger = logger;
    }

    public Task CopyFromSftpAsync(string host, int port, string username, string password, 
        string remotePath, string localPath, bool recursive)
    {
        return Task.Run(() =>
        {
            _logger.LogInformation("🔌 Connexion au serveur SFTP {Host}:{Port}", host, port);

            using var client = new SftpClient(host, port, username, password);
            
            try
            {
                client.Connect();
                _logger.LogInformation("✅ Connexion établie avec succès");

                // Vérifier si le chemin distant existe
                if (!client.Exists(remotePath))
                {
                    throw new FileNotFoundException($"Le chemin distant '{remotePath}' n'existe pas");
                }

                var remoteItem = client.Get(remotePath);
                
                if (remoteItem.IsDirectory)
                {
                    if (recursive)
                    {
                        _logger.LogInformation("📂 Copie récursive du dossier '{RemotePath}' vers '{LocalPath}'", remotePath, localPath);
                        CopyDirectory(client, remotePath, localPath);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Le chemin '{remotePath}' est un dossier. Utilisez --recursive pour copier les dossiers.");
                    }
                }
                else
                {
                    _logger.LogInformation("📄 Copie du fichier '{RemotePath}' vers '{LocalPath}'", remotePath, localPath);
                    CopyFile(client, remotePath, localPath);
                }

                _logger.LogInformation("🎉 Copie terminée avec succès");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la copie SFTP");
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                    _logger.LogInformation("🔌 Déconnexion du serveur SFTP");
                }
            }
        });
    }

    private void CopyFile(SftpClient client, string remoteFilePath, string localPath)
    {
        var fileName = Path.GetFileName(remoteFilePath);
        string localFilePath;

        // Déterminer le chemin de destination local
        if (Directory.Exists(localPath))
        {
            // Si localPath est un dossier existant, copier le fichier dedans
            localFilePath = Path.Combine(localPath, fileName);
        }
        else
        {
            // Si localPath n'existe pas, l'utiliser comme nom de fichier de destination
            localFilePath = localPath;
            var localDir = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(localDir) && !Directory.Exists(localDir))
            {
                Directory.CreateDirectory(localDir);
                _logger.LogInformation("📁 Dossier créé : {LocalDir}", localDir);
            }
        }

        // Obtenir la taille du fichier pour calculer le pourcentage
        var remoteFile = client.Get(remoteFilePath);
        var totalSize = remoteFile.Length;
        
        _logger.LogInformation("⬇️ Téléchargement : {RemoteFile} -> {LocalFile} ({Size} octets)", 
            remoteFilePath, localFilePath, totalSize);

        using var fileStream = File.Create(localFilePath);
        
        // Téléchargement avec callback de progression
        client.DownloadFile(remoteFilePath, fileStream, CreateProgressCallback(totalSize));

        _logger.LogInformation("✅ Fichier copié : {Size} octets", totalSize);
    }

    private void CopyDirectory(SftpClient client, string remoteDirPath, string localDirPath)
    {
        // Créer le dossier local s'il n'existe pas
        if (!Directory.Exists(localDirPath))
        {
            Directory.CreateDirectory(localDirPath);
            _logger.LogInformation("📁 Dossier créé : {LocalDir}", localDirPath);
        }

        // Lister le contenu du dossier distant
        var remoteFiles = client.ListDirectory(remoteDirPath);

        foreach (var remoteFile in remoteFiles)
        {
            // Ignorer les entrées . et ..
            if (remoteFile.Name == "." || remoteFile.Name == "..")
                continue;

            var remoteItemPath = $"{remoteDirPath.TrimEnd('/')}/{remoteFile.Name}";
            var localItemPath = Path.Combine(localDirPath, remoteFile.Name);

            if (remoteFile.IsDirectory)
            {
                _logger.LogInformation("📂 Traitement du dossier : {RemoteDir}", remoteItemPath);
                CopyDirectory(client, remoteItemPath, localItemPath);
            }
            else if (remoteFile.IsRegularFile)
            {
                // Obtenir la taille du fichier pour calculer le pourcentage
                var totalSize = remoteFile.Length;
                
                _logger.LogInformation("⬇️ Téléchargement : {RemoteFile} -> {LocalFile} ({Size} octets)", 
                    remoteItemPath, localItemPath, totalSize);
                
                using var fileStream = File.Create(localItemPath);
                
                // Téléchargement avec callback de progression
                client.DownloadFile(remoteItemPath, fileStream, CreateProgressCallback(totalSize));
                
                _logger.LogInformation("✅ Fichier copié : {Size} octets", totalSize);
            }
        }
    }

    private Action<ulong> CreateProgressCallback(long totalSize)
    {
        // Variables pour le suivi de progression (closure)
        long lastReportedBytes = 0;
        var lastProgressTime = DateTime.Now;
        
        return (ulong bytesDownloaded) =>
        {
            var now = DateTime.Now;
            var downloaded = (long)bytesDownloaded;
            
            // Mettre à jour la progression toutes les 500ms ou tous les 10% 
            var timeDiff = now - lastProgressTime;
            var byteDiff = downloaded - lastReportedBytes;
            var percentageDiff = totalSize > 0 ? (byteDiff * 100.0 / totalSize) : 0;
            
            if (timeDiff.TotalMilliseconds >= 500 || percentageDiff >= 10 || downloaded == totalSize)
            {
                if (totalSize > 0)
                {
                    var percentage = (downloaded * 100.0 / totalSize);
                    var speed = byteDiff > 0 && timeDiff.TotalSeconds > 0 ? 
                        (long)(byteDiff / timeDiff.TotalSeconds) : 0;
                    
                    _logger.LogInformation("📊 Progression : {Percentage:F1}% ({Downloaded}/{Total}) - {Speed}/s", 
                        percentage, 
                        FormatBytes(downloaded), 
                        FormatBytes(totalSize),
                        FormatBytes(speed));
                }
                
                lastReportedBytes = downloaded;
                lastProgressTime = now;
            }
        };
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1}{suffixes[counter]}";
    }
}
