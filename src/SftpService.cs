namespace SftpCopyTool;

using Microsoft.Extensions.Logging;

using Renci.SshNet;

/// <summary>
///     Service de copie de fichiers via SFTP.
/// </summary>
public sealed class SftpService
{
    private readonly ILogger<SftpService> logger;

    /// <summary>Initialise une nouvelle instance de la class <see cref="SftpService"/>.</summary>
    /// <param name="logger">Logger pour les opérations de logging.</param>
    /// <exception cref="ArgumentNullException">Levée quand logger est null.</exception>
    public SftpService(ILogger<SftpService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Copie des fichiers depuis un serveur SFTP vers le système local.</summary>
    /// <param name="host">Adresse du serveur SFTP.</param>
    /// <param name="port">Port du serveur SFTP.</param>
    /// <param name="username">Nom d'utilisateur pour l'authentification.</param>
    /// <param name="password">Mot de passe pour l'authentification.</param>
    /// <param name="remotePath">Chemin distant à copier.</param>
    /// <param name="localPath">Chemin local de destination.</param>
    /// <param name="recursive">Indique si la copie doit être récursive pour les dossiers.</param>
    /// <param name="cancellationToken">Token d'annulation pour l'opération asynchrone.</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    /// <exception cref="ArgumentException">Levée quand un paramètre requis est null ou vide.</exception>
    /// <exception cref="FileNotFoundException">Levée quand le chemin distant n'existe pas.</exception>
    /// <exception cref="InvalidOperationException">Levée quand le chemin distant est un dossier mais recursive est false.</exception>
    public async Task CopyFromSftpAsync(string host, int port, string username, string password,
        string remotePath, string localPath, bool recursive, CancellationToken cancellationToken = default)
    {
        // Validation des paramètres
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(remotePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(localPath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(port);

        this.logger.LogInformation("🔌 Connexion au serveur SFTP {Host}:{Port}", host, port);

        using var client = new SftpClient(host, port, username, password);

        try
        {
            await Task.Run(client.Connect, cancellationToken);
            this.logger.LogInformation("✅ Connexion établie avec succès");

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
                    this.logger.LogInformation("📂 Copie récursive du dossier '{RemotePath}' vers '{LocalPath}'", remotePath, localPath);
                    await CopyDirectoryAsync(client, remotePath, localPath, cancellationToken);
                }
                else
                {
                    throw new InvalidOperationException($"Le chemin '{remotePath}' est un dossier. Utilisez --recursive pour copier les dossiers.");
                }
            }
            else
            {
                this.logger.LogInformation("📄 Copie du fichier '{RemotePath}' vers '{LocalPath}'", remotePath, localPath);
                await CopyFileAsync(client, remotePath, localPath, cancellationToken);
            }

            this.logger.LogInformation("🎉 Copie terminée avec succès");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "❌ Erreur lors de la copie SFTP de {RemotePath}", remotePath);
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                client.Disconnect();
                this.logger.LogInformation("🔌 Déconnexion du serveur SFTP");
            }
        }
    }

    private async Task CopyFileAsync(SftpClient client, string remoteFilePath, string localPath, CancellationToken cancellationToken)
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
                this.logger.LogInformation("📁 Dossier créé : {LocalDir}", localDir);
            }
        }

        // Obtenir la taille du fichier pour calculer le pourcentage
        var remoteFile = client.Get(remoteFilePath);
        var totalSize = remoteFile.Length;

        this.logger.LogInformation("⬇️ Téléchargement : {RemoteFile} -> {LocalFile} ({Size} octets)",
            remoteFilePath, localFilePath, totalSize);

        using var fileStream = File.Create(localFilePath);

        // Téléchargement avec callback de progression
        await Task.Run(() =>
            client.DownloadFile(remoteFilePath, fileStream, CreateProgressCallback(totalSize)),
            cancellationToken);

        this.logger.LogInformation("✅ Fichier copié : {Size} octets", totalSize);
    }

    private async Task CopyDirectoryAsync(SftpClient client, string remoteDirPath, string localDirPath, CancellationToken cancellationToken)
    {
        // Créer le dossier local s'il n'existe pas
        if (!Directory.Exists(localDirPath))
        {
            Directory.CreateDirectory(localDirPath);
            this.logger.LogInformation("📁 Dossier créé : {LocalDir}", localDirPath);
        }

        // Lister le contenu du dossier distant
        var remoteFiles = client.ListDirectory(remoteDirPath);

        foreach (var remoteFile in remoteFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ignorer les entrées . et ..
            if (remoteFile.Name is "." or "..")
                continue;

            var remoteItemPath = $"{remoteDirPath.TrimEnd('/')}/{remoteFile.Name}";
            var localItemPath = Path.Combine(localDirPath, remoteFile.Name);

            if (remoteFile.IsDirectory)
            {
                this.logger.LogInformation("📂 Traitement du dossier : {RemoteDir}", remoteItemPath);
                await CopyDirectoryAsync(client, remoteItemPath, localItemPath, cancellationToken);
            }
            else if (remoteFile.IsRegularFile)
            {
                // Obtenir la taille du fichier pour calculer le pourcentage
                var totalSize = remoteFile.Length;

                this.logger.LogInformation("⬇️ Téléchargement : {RemoteFile} -> {LocalFile} ({Size} octets)",
                    remoteItemPath, localItemPath, totalSize);

                using var fileStream = File.Create(localItemPath);

                // Téléchargement avec callback de progression
                await Task.Run(() =>
                    client.DownloadFile(remoteItemPath, fileStream, CreateProgressCallback(totalSize)),
                    cancellationToken);

                this.logger.LogInformation("✅ Fichier copié : {Size} octets", totalSize);
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

                    this.logger.LogInformation("📊 Progression : {Percentage:F1}% ({Downloaded}/{Total}) - {Speed}/s",
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
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
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
