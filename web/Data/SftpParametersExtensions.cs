using SftpCopyTool.Web.Services;

namespace SftpCopyTool.Web.Data;

/// <summary>
/// Méthodes d'extension pour la conversion entre SftpParameters et SavedSftpParameters
/// </summary>
public static class SftpParametersExtensions
{
    /// <summary>
    /// Convertit SftpParameters vers SavedSftpParameters (sans mot de passe)
    /// </summary>
    public static SavedSftpParameters ToSaved(this SftpParameters parameters)
    {
        return new SavedSftpParameters
        {
            Host = parameters.Host,
            Port = parameters.Port,
            Username = parameters.Username,
            RemotePath = parameters.RemotePath,
            LocalPath = parameters.LocalPath,
            Recursive = parameters.Recursive,
            LogLevel = parameters.LogLevel,
            EnableFileLogging = parameters.EnableFileLogging,
            LastSaved = DateTime.Now
        };
    }

    /// <summary>
    /// Applique les paramètres sauvegardés à SftpParameters (préserve le mot de passe existant)
    /// </summary>
    public static void ApplyFrom(this SftpParameters parameters, SavedSftpParameters saved)
    {
        parameters.Host = saved.Host;
        parameters.Port = saved.Port;
        parameters.Username = saved.Username;
        parameters.RemotePath = saved.RemotePath;
        parameters.LocalPath = saved.LocalPath;
        parameters.Recursive = saved.Recursive;
        parameters.LogLevel = saved.LogLevel;
        parameters.EnableFileLogging = saved.EnableFileLogging;
        // Le mot de passe n'est pas modifié pour préserver la sécurité
    }
}
