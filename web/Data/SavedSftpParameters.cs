namespace SftpCopyTool.Web.Data;

/// <summary>
/// Paramètres SFTP sauvegardés dans le localStorage (sans mot de passe pour la sécurité)
/// </summary>
public class SavedSftpParameters
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;
    public string RemotePath { get; set; } = "/";
    public string LocalPath { get; set; } = "./downloads";
    public bool Recursive { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// Date de dernière sauvegarde
    /// </summary>
    public DateTime LastSaved { get; set; } = DateTime.Now;
}
