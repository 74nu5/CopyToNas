// <copyright file="IProgressReporter.cs" company="CopyToNas">
// Copyright (c) CopyToNas. Tous droits réservés.
// </copyright>

namespace SftpCopyTool.Web.Services;

/// <summary>Interface pour reporter la progression des opérations SFTP.</summary>
public interface IProgressReporter
{
    /// <summary>Obtient l'état actuel de la progression.</summary>
    ProgressState CurrentState { get; }

    /// <summary>Événement déclenché lors d'une mise à jour de la progression.</summary>
    event Action<ProgressState>? OnProgressUpdate;

    /// <summary>Démarre une nouvelle opération.</summary>
    /// <param name="operation">Description de l'opération.</param>
    void StartOperation(string operation);

    /// <summary>Met à jour la progression de l'opération actuelle.</summary>
    /// <param name="progress">Pourcentage de progression (0-100).</param>
    /// <param name="message">Message de statut optionnel.</param>
    void UpdateProgress(double progress, string? message = null);

    /// <summary>Démarre la progression d'un nouveau fichier.</summary>
    /// <param name="fileName">Nom du fichier.</param>
    /// <param name="totalSize">Taille totale du fichier.</param>
    void StartFile(string fileName, long totalSize);

    /// <summary>Met à jour la progression du fichier actuel.</summary>
    /// <param name="bytesTransferred">Nombre d'octets transférés.</param>
    /// <param name="speed">Vitesse de transfert en octets par seconde.</param>
    void UpdateFileProgress(long bytesTransferred, double speed = 0);

    /// <summary>Termine la progression du fichier actuel.</summary>
    void CompleteFile();

    /// <summary>Ajoute un message de log.</summary>
    /// <param name="level">Niveau de log.</param>
    /// <param name="message">Message à logger.</param>
    void AddLogMessage(LogLevel level, string message);

    /// <summary>Termine l'opération actuelle.</summary>
    /// <param name="success">Indique si l'opération a réussi.</param>
    /// <param name="finalMessage">Message final optionnel.</param>
    void CompleteOperation(bool success, string? finalMessage = null);

    /// <summary>Remet à zéro l'état de progression.</summary>
    void Reset();
}

/// <summary>État de progression d'une opération.</summary>
public class ProgressState
{
    /// <summary>Nom de l'opération en cours.</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Pourcentage de progression (0-100).</summary>
    public double Progress { get; set; }

    /// <summary>Message de statut actuel.</summary>
    public string? CurrentMessage { get; set; }

    /// <summary>Indique si une opération est en cours.</summary>
    public bool IsRunning { get; set; }

    /// <summary>Indique si la dernière opération a réussi.</summary>
    public bool? LastOperationSuccess { get; set; }

    /// <summary>Historique des messages de log.</summary>
    public List<LogMessage> LogMessages { get; set; } = new();

    /// <summary>Heure de début de l'opération.</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>Heure de fin de l'opération.</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Informations sur le fichier en cours de traitement.</summary>
    public FileProgressInfo? CurrentFile { get; set; }
}

/// <summary>Informations sur la progression d'un fichier individuel.</summary>
public class FileProgressInfo
{
    /// <summary>Nom du fichier en cours de traitement.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Pourcentage de progression du fichier (0-100).</summary>
    public double Progress { get; set; }

    /// <summary>Nombre d'octets transférés.</summary>
    public long BytesTransferred { get; set; }

    /// <summary>Taille totale du fichier en octets.</summary>
    public long TotalSize { get; set; }

    /// <summary>Vitesse de transfert en octets par seconde.</summary>
    public double Speed { get; set; }

    /// <summary>Temps restant estimé.</summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>Formatage des octets en unités lisibles.</summary>
    /// <param name="bytes">Nombre d'octets.</param>
    /// <returns>Représentation formatée.</returns>
    public static string FormatBytes(long bytes)
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

    /// <summary>Obtient le texte formaté des octets transférés.</summary>
    public string FormattedBytesTransferred => FormatBytes(BytesTransferred);

    /// <summary>Obtient le texte formaté de la taille totale.</summary>
    public string FormattedTotalSize => FormatBytes(TotalSize);

    /// <summary>Obtient le texte formaté de la vitesse.</summary>
    public string FormattedSpeed => FormatBytes((long)Speed) + "/s";
}

/// <summary>Message de log avec niveau et timestamp.</summary>
public class LogMessage
{
    /// <summary>Niveau du message.</summary>
    public LogLevel Level { get; set; }

    /// <summary>Contenu du message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Timestamp du message.</summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>Obtient la classe CSS Bootstrap pour le niveau de log.</summary>
    public string BootstrapClass => Level switch
    {
        LogLevel.Error or LogLevel.Critical => "text-danger",
        LogLevel.Warning => "text-warning",
        LogLevel.Information => "text-info",
        LogLevel.Debug => "text-muted",
        _ => "text-secondary"
    };

    /// <summary>Obtient l'icône pour le niveau de log.</summary>
    public string Icon => Level switch
    {
        LogLevel.Error or LogLevel.Critical => "🚨",
        LogLevel.Warning => "⚠️",
        LogLevel.Information => "ℹ️",
        LogLevel.Debug => "🔍",
        _ => "📝"
    };
}
