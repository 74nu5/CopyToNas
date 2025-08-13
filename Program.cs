using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.CommandLine;

namespace SftpCopyTool;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configuration du logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<Program>();

        // Configuration des arguments de ligne de commande
        var hostOption = new Option<string>(
            name: "--host",
            description: "Adresse du serveur SFTP")
        { IsRequired = true };

        var portOption = new Option<int>(
            name: "--port",
            description: "Port du serveur SFTP",
            getDefaultValue: () => 22);

        var usernameOption = new Option<string>(
            name: "--username",
            description: "Nom d'utilisateur SFTP")
        { IsRequired = true };

        var passwordOption = new Option<string>(
            name: "--password",
            description: "Mot de passe SFTP")
        { IsRequired = true };

        var remotePathOption = new Option<string>(
            name: "--remote-path",
            description: "Chemin absolu sur le serveur distant")
        { IsRequired = true };

        var localPathOption = new Option<string>(
            name: "--local-path",
            description: "Dossier local de destination")
        { IsRequired = true };

        var recursiveOption = new Option<bool>(
            name: "--recursive",
            description: "Copie récursive des dossiers et sous-dossiers",
            getDefaultValue: () => false);

        var rootCommand = new RootCommand("Outil de copie SFTP vers local")
        {
            hostOption,
            portOption,
            usernameOption,
            passwordOption,
            remotePathOption,
            localPathOption,
            recursiveOption
        };

        rootCommand.SetHandler(async (host, port, username, password, remotePath, localPath, recursive) =>
        {
            try
            {
                var sftpLogger = loggerFactory.CreateLogger<SftpService>();
                var sftpService = new SftpService(sftpLogger);
                await sftpService.CopyFromSftpAsync(host, port, username, password, remotePath, localPath, recursive);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la copie SFTP");
                Environment.Exit(1);
            }
        }, hostOption, portOption, usernameOption, passwordOption, remotePathOption, localPathOption, recursiveOption);

        return await rootCommand.InvokeAsync(args);
    }
}
