// <copyright file="ProgressReporter.cs" company="CopyToNas">
// Copyright (c) CopyToNas. Tous droits réservés.
// </copyright>

namespace SftpCopyTool.Web.Services;

/// <summary>Implémentation du reporter de progression pour les opérations SFTP.</summary>
public class ProgressReporter : IProgressReporter
{
    private readonly object lockObject = new();

    /// <inheritdoc/>
    public ProgressState CurrentState { get; private set; } = new();

    /// <inheritdoc/>
    public event Action<ProgressState>? OnProgressUpdate;

    /// <inheritdoc/>
    public void StartOperation(string operation)
    {
        lock (this.lockObject)
        {
            this.CurrentState = new ProgressState
            {
                Operation = operation,
                Progress = 0,
                IsRunning = true,
                StartTime = DateTime.Now,
                LogMessages = new List<LogMessage>()
            };

            this.AddLogMessage(LogLevel.Information, $"🚀 Démarrage de l'opération : {operation}");
        }

        this.OnProgressUpdate?.Invoke(this.CurrentState);
    }

    /// <inheritdoc/>
    public void UpdateProgress(double progress, string? message = null)
    {
        lock (this.lockObject)
        {
            this.CurrentState.Progress = Math.Clamp(progress, 0, 100);

            if (!string.IsNullOrEmpty(message))
            {
                this.CurrentState.CurrentMessage = message;
                this.AddLogMessage(LogLevel.Information, message);
            }
        }

        this.OnProgressUpdate?.Invoke(this.CurrentState);
    }

    /// <inheritdoc/>
    public void StartFile(string fileName, long totalSize)
    {
        lock (this.lockObject)
        {
            this.CurrentState.CurrentFile = new FileProgressInfo
            {
                FileName = fileName,
                Progress = 0,
                BytesTransferred = 0,
                TotalSize = totalSize,
                Speed = 0
            };
        }

        this.OnProgressUpdate?.Invoke(this.CurrentState);
    }

    /// <inheritdoc/>
    public void UpdateFileProgress(long bytesTransferred, double speed = 0)
    {
        lock (this.lockObject)
        {
            if (this.CurrentState.CurrentFile != null)
            {
                this.CurrentState.CurrentFile.BytesTransferred = bytesTransferred;
                this.CurrentState.CurrentFile.Speed = speed;
                this.CurrentState.CurrentFile.Progress = this.CurrentState.CurrentFile.TotalSize > 0
                    ? (double)bytesTransferred / this.CurrentState.CurrentFile.TotalSize * 100
                    : 0;

                // Calculer le temps restant estimé si on a une vitesse
                if (speed > 0)
                {
                    var remainingBytes = this.CurrentState.CurrentFile.TotalSize - bytesTransferred;
                    this.CurrentState.CurrentFile.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingBytes / speed);
                }
            }
        }

        this.OnProgressUpdate?.Invoke(this.CurrentState);
    }

    /// <inheritdoc/>
    public void CompleteFile()
    {
        lock (this.lockObject)
        {
            if (this.CurrentState.CurrentFile != null)
            {
                this.CurrentState.CurrentFile.Progress = 100;
                this.CurrentState.CurrentFile.BytesTransferred = this.CurrentState.CurrentFile.TotalSize;
                this.CurrentState.CurrentFile.EstimatedTimeRemaining = TimeSpan.Zero;
            }
        }

        this.OnProgressUpdate?.Invoke(this.CurrentState);
    }

    /// <inheritdoc/>
    public void AddLogMessage(LogLevel level, string message)
    {
        lock (this.lockObject)
        {
            this.CurrentState.LogMessages.Add(new LogMessage
            {
                Level = level,
                Message = message,
                Timestamp = DateTime.Now
            });

            // Garder seulement les 1000 derniers messages pour éviter la surcharge mémoire
            if (this.CurrentState.LogMessages.Count > 1000)
            {
                this.CurrentState.LogMessages.RemoveRange(0, this.CurrentState.LogMessages.Count - 1000);
            }
        }

        this.OnProgressUpdate?.Invoke(this.CurrentState);
    }

    /// <inheritdoc/>
    public void CompleteOperation(bool success, string? finalMessage = null)
    {
        lock (this.lockObject)
        {
            this.CurrentState.IsRunning = false;
            this.CurrentState.LastOperationSuccess = success;
            this.CurrentState.EndTime = DateTime.Now;
            this.CurrentState.Progress = 100;

            var statusIcon = success ? "✅" : "❌";
            var statusText = success ? "réussie" : "échouée";
            var defaultMessage = $"{statusIcon} Opération {statusText}";

            var message = finalMessage ?? defaultMessage;
            this.CurrentState.CurrentMessage = message;

            this.AddLogMessage(success ? LogLevel.Information : LogLevel.Error, message);

            if (this.CurrentState.StartTime.HasValue)
            {
                var duration = DateTime.Now - this.CurrentState.StartTime.Value;
                this.AddLogMessage(LogLevel.Information, $"⏱️ Durée totale : {duration:mm\\:ss\\.fff}");
            }
        }

        this.OnProgressUpdate?.Invoke(this.CurrentState);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        lock (this.lockObject)
        {
            this.CurrentState = new ProgressState();
        }

        this.OnProgressUpdate?.Invoke(this.CurrentState);
    }
}
