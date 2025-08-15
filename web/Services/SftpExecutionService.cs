// <copyright file="SftpExecutionService.cs" company="CopyToNas">
// Copyright (c) CopyToNas. Tous droits réservés.
// </copyright>

using Tmds.Ssh;

namespace SftpCopyTool.Web.Services;

/// <summary>Service d'exécution des opérations SFTP avec reporting de progression.</summary>
public class SftpExecutionService
{
    private readonly SftpService sftpService;
    private readonly IProgressReporter progressReporter;
    private readonly ILogger<SftpExecutionService> logger;

    /// <summary>Initialise une nouvelle instance du service d'exécution SFTP.</summary>
    /// <param name="sftpService">Service SFTP pour les opérations.</param>
    /// <param name="progressReporter">Reporter de progression.</param>
    /// <param name="logger">Logger pour les opérations.</param>
    public SftpExecutionService(
        SftpService sftpService,
        IProgressReporter progressReporter,
        ILogger<SftpExecutionService> logger)
    {
        this.sftpService = sftpService ?? throw new ArgumentNullException(nameof(sftpService));
        this.progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Exécute une opération de copie SFTP de manière asynchrone.</summary>
    /// <param name="parameters">Paramètres de connexion et de copie.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    public async Task ExecuteCopyAsync(SftpParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var operationName = $"Copie SFTP : {parameters.RemotePath} → {parameters.LocalPath}";

        try
        {
            this.progressReporter.StartOperation(operationName);
            this.progressReporter.UpdateProgress(5, "🔍 Validation des paramètres...");

            // Validation des paramètres
            this.ValidateParameters(parameters);

            this.progressReporter.UpdateProgress(10, "🔗 Connexion au serveur SFTP...");

            // Créer un service SFTP custom avec callback de progression
            var progressLogger = new ProgressLogger(this.progressReporter, this.logger);
            var sftpWithProgress = new SftpService(progressLogger);

            // Exécuter la copie
            await sftpWithProgress.CopyFromSftpAsync(
                parameters.Host,
                parameters.Port,
                parameters.Username,
                parameters.Password,
                parameters.RemotePath,
                parameters.LocalPath,
                parameters.Recursive,
                cancellationToken);

            this.progressReporter.CompleteOperation(true, "🎉 Copie terminée avec succès !");
        }
        catch (OperationCanceledException)
        {
            this.progressReporter.CompleteOperation(false, "⏹️ Opération annulée par l'utilisateur");
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Erreur lors de l'exécution de la copie SFTP");
            this.progressReporter.CompleteOperation(false, $"❌ Erreur : {ex.Message}");
            throw;
        }
    }

    /// <summary>Teste la connexion SFTP avec les paramètres fournis.</summary>
    /// <param name="parameters">Paramètres de connexion à tester.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche représentant l'opération asynchrone avec le résultat du test.</returns>
    public async Task<(bool Success, string Message)> TestConnectionAsync(SftpParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var operationName = $"Test de connexion SFTP : {parameters.Host}:{parameters.Port}";

        try
        {
            this.progressReporter.StartOperation(operationName);
            this.progressReporter.UpdateProgress(10, "🔍 Validation des paramètres de connexion...");

            // Validation des paramètres de connexion uniquement
            this.ValidateConnectionParameters(parameters);

            this.progressReporter.UpdateProgress(30, "🔗 Tentative de connexion...");
            this.progressReporter.UpdateProgress(50, "🔐 Authentification...");

            // Tester la connexion en utilisant Tmds.Ssh
            var settings = new SshClientSettings($"{parameters.Username}@{parameters.Host}:{parameters.Port}")
            {
                Credentials = [new PasswordCredential(parameters.Password)]
            };

            using var sshClient = new SshClient(settings);
            await sshClient.ConnectAsync(cancellationToken);

            this.progressReporter.UpdateProgress(80, "📂 Vérification des permissions...");

            // Test simple : ouvrir un client SFTP et lister un répertoire
            using var sftpClient = await sshClient.OpenSftpClientAsync(cancellationToken);
            var testPath = string.IsNullOrWhiteSpace(parameters.RemotePath) ? "/" : parameters.RemotePath;

            try
            {
                await foreach (var entry in sftpClient.GetDirectoryEntriesAsync(testPath))
                {
                    // Juste tester qu'on peut lire le premier élément
                    break;
                }
                this.progressReporter.UpdateProgress(90, "📋 Test de lecture réussi");
            }
            catch (Exception ex)
            {
                // Si on ne peut pas lire le répertoire, ce n'est pas forcément grave
                this.progressReporter.AddLogMessage(LogLevel.Warning, $"⚠️ Impossible de lire {testPath} : {ex.Message}");
            }

            this.progressReporter.CompleteOperation(true, "✅ Test de connexion réussi !");
            return (true, "Connexion établie avec succès");
        }
        catch (OperationCanceledException)
        {
            this.progressReporter.CompleteOperation(false, "⏹️ Test de connexion annulé");
            return (false, "Test de connexion annulé par l'utilisateur");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Erreur lors du test de connexion SFTP");
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            this.progressReporter.CompleteOperation(false, $"❌ Échec de la connexion : {errorMessage}");
            return (false, $"Échec de la connexion : {errorMessage}");
        }
    }

    private void ValidateConnectionParameters(SftpParameters parameters)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(parameters.Host))
            errors.Add("L'adresse du serveur est requise");

        if (parameters.Port <= 0 || parameters.Port > 65535)
            errors.Add("Le port doit être compris entre 1 et 65535");

        if (string.IsNullOrWhiteSpace(parameters.Username))
            errors.Add("Le nom d'utilisateur est requis");

        if (string.IsNullOrWhiteSpace(parameters.Password))
            errors.Add("Le mot de passe est requis");

        if (errors.Count > 0)
        {
            var errorMessage = string.Join(", ", errors);
            this.progressReporter.AddLogMessage(LogLevel.Error, $"❌ Erreurs de validation : {errorMessage}");
            throw new ArgumentException($"Paramètres de connexion invalides : {errorMessage}");
        }

        this.progressReporter.AddLogMessage(LogLevel.Information, "✅ Paramètres de connexion validés");
    }

    private void ValidateParameters(SftpParameters parameters)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(parameters.Host))
            errors.Add("L'adresse du serveur est requise");

        if (parameters.Port <= 0 || parameters.Port > 65535)
            errors.Add("Le port doit être compris entre 1 et 65535");

        if (string.IsNullOrWhiteSpace(parameters.Username))
            errors.Add("Le nom d'utilisateur est requis");

        if (string.IsNullOrWhiteSpace(parameters.Password))
            errors.Add("Le mot de passe est requis");

        if (string.IsNullOrWhiteSpace(parameters.RemotePath))
            errors.Add("Le chemin distant est requis");

        if (string.IsNullOrWhiteSpace(parameters.LocalPath))
            errors.Add("Le chemin local est requis");

        if (errors.Count > 0)
        {
            var errorMessage = string.Join(", ", errors);
            this.progressReporter.AddLogMessage(LogLevel.Error, $"❌ Erreurs de validation : {errorMessage}");
            throw new ArgumentException($"Paramètres invalides : {errorMessage}");
        }

        this.progressReporter.AddLogMessage(LogLevel.Information, "✅ Paramètres validés avec succès");
    }
}

/// <summary>Paramètres pour une opération SFTP.</summary>
public class SftpParameters
{
    /// <summary>Adresse du serveur SFTP.</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>Port du serveur SFTP.</summary>
    public int Port { get; set; } = 22;

    /// <summary>Nom d'utilisateur.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Mot de passe.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Chemin distant à copier.</summary>
    public string RemotePath { get; set; } = "/";

    /// <summary>Chemin local de destination.</summary>
    public string LocalPath { get; set; } = "./downloads";

    /// <summary>Copie récursive des dossiers.</summary>
    public bool Recursive { get; set; } = true;

    /// <summary>Niveau de logging.</summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>Activer l'écriture des logs dans un fichier.</summary>
    public bool EnableFileLogging { get; set; } = true;
}

/// <summary>Logger qui reporte la progression vers le ProgressReporter.</summary>
internal class ProgressLogger : ILogger<SftpService>
{
    private readonly IProgressReporter progressReporter;
    private readonly ILogger fallbackLogger;

    public ProgressLogger(IProgressReporter progressReporter, ILogger fallbackLogger)
    {
        this.progressReporter = progressReporter;
        this.fallbackLogger = fallbackLogger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this.fallbackLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        // Reporter vers le ProgressReporter
        this.progressReporter.AddLogMessage(logLevel, message);

        // Reporter vers le logger standard aussi
        this.fallbackLogger.Log(logLevel, eventId, state, exception, formatter);

        // Mise à jour de la progression basée sur le contenu du message
        if (message.Contains("Progression :") && message.Contains("%"))
        {
            // Extraire le pourcentage du message (ex: "Progression : 45.2%")
            var percentMatch = System.Text.RegularExpressions.Regex.Match(message, @"(\d+\.?\d*)\s*%");
            if (percentMatch.Success && double.TryParse(percentMatch.Groups[1].Value, out var percent))
            {
                this.progressReporter.UpdateProgress(percent, message);
            }
        }
    }
}
