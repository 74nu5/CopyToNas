namespace SftpCopyTool;

using Microsoft.Extensions.Logging;
using Tmds.Ssh;

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

        var settings = new SshClientSettings($"{username}@{host}:{port}")
        {
            Credentials = [new PasswordCredential(password)],
        };

        using var sshClient = new SshClient(settings);

        try
        {
            await sshClient.ConnectAsync(cancellationToken);
            this.logger.LogInformation("✅ Connexion établie avec succès");

            using var sftpClient = await sshClient.OpenSftpClientAsync(cancellationToken);

            // Vérifier si le chemin distant existe et obtenir ses propriétés
            var remoteAttributes = await sftpClient.GetAttributesAsync(remotePath, followLinks: true, cancellationToken);

            if (remoteAttributes?.FileType == UnixFileType.Directory)
            {
                if (recursive)
                {
                    this.logger.LogInformation("📂 Copie récursive du dossier '{RemotePath}' vers '{LocalPath}'", remotePath, localPath);
                    await CopyDirectoryAsync(sftpClient, remotePath, localPath, cancellationToken);
                }
                else
                {
                    throw new InvalidOperationException($"Le chemin '{remotePath}' est un dossier. Utilisez --recursive pour copier les dossiers.");
                }
            }
            else
            {
                this.logger.LogInformation("📄 Copie du fichier '{RemotePath}' vers '{LocalPath}'", remotePath, localPath);
                await CopyFileAsync(sftpClient, remotePath, localPath, cancellationToken);
            }

            this.logger.LogInformation("🎉 Copie terminée avec succès");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "❌ Erreur lors de la copie SFTP de {RemotePath}", remotePath);
            throw;
        }
    }

    private async Task CopyFileAsync(SftpClient sftpClient, string remoteFilePath, string localPath, CancellationToken cancellationToken)
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
        var attributes = await sftpClient.GetAttributesAsync(remoteFilePath, followLinks: true, cancellationToken);
        var totalSize = attributes?.Length ?? 0;

        this.logger.LogInformation("⬇️ Téléchargement : {RemoteFile} -> {LocalFile} ({Size})",
            remoteFilePath, localFilePath, FormatBytes(totalSize));

        // Téléchargement avec suivi de progression
        var startTime = DateTime.UtcNow;

        using var fileStream = File.Create(localFilePath);
        using var progressStream = new ProgressTrackingStream(fileStream, fileName, totalSize, this.logger);

        await sftpClient.DownloadFileAsync(remoteFilePath, progressStream, cancellationToken);

        var duration = DateTime.UtcNow - startTime;
        var speed = totalSize > 0 && duration.TotalSeconds > 0 ? totalSize / duration.TotalSeconds : 0;

        this.logger.LogInformation("✅ Fichier copié : {Size} en {Duration:F1}s ({Speed}/s)",
            FormatBytes(totalSize), duration.TotalSeconds, FormatBytes((long)speed));
    }

    private async Task CopyDirectoryAsync(SftpClient sftpClient, string remoteDirPath, string localDirPath, CancellationToken cancellationToken)
    {
        // Créer le dossier local s'il n'existe pas
        if (!Directory.Exists(localDirPath))
        {
            Directory.CreateDirectory(localDirPath);
            this.logger.LogInformation("📁 Dossier créé : {LocalDir}", localDirPath);
        }

        // Lister le contenu du dossier distant
        await foreach (var (entryPath, entryAttributes) in sftpClient.GetDirectoryEntriesAsync(remoteDirPath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entryName = Path.GetFileName(entryPath);

            // Ignorer les entrées . et ..
            if (entryName is "." or "..")
                continue;

            var remoteItemPath = entryPath;
            var localItemPath = Path.Combine(localDirPath, entryName);

            if (entryAttributes.FileType == UnixFileType.Directory)
            {
                this.logger.LogInformation("📂 Traitement du dossier : {RemoteDir}", remoteItemPath);
                await CopyDirectoryAsync(sftpClient, remoteItemPath, localItemPath, cancellationToken);
            }
            else if (entryAttributes.FileType == UnixFileType.RegularFile)
            {
                // Obtenir la taille du fichier
                var totalSize = entryAttributes.Length;

                this.logger.LogInformation("⬇️ Téléchargement : {RemoteFile} -> {LocalFile} ({Size})",
                    remoteItemPath, localItemPath, FormatBytes(totalSize));

                var startTime = DateTime.UtcNow;

                using var fileStream = File.Create(localItemPath);
                using var progressStream = new ProgressTrackingStream(fileStream, entryName, totalSize, this.logger);

                await sftpClient.DownloadFileAsync(remoteItemPath, progressStream, cancellationToken);

                var duration = DateTime.UtcNow - startTime;
                var speed = totalSize > 0 && duration.TotalSeconds > 0 ? totalSize / duration.TotalSeconds : 0;

                this.logger.LogInformation("✅ Fichier copié : {Size} en {Duration:F1}s ({Speed}/s)",
                    FormatBytes(totalSize), duration.TotalSeconds, FormatBytes((long)speed));
            }
        }
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
