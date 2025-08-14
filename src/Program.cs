using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.CommandLine;
using Serilog;
using Serilog.Extensions.Logging;

namespace SftpCopyTool
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Configuration de Serilog par défaut (sera reconfigurée après analyse des arguments)
            ConfigureSerilog("Information", true);

            var hostOption = new Option<string>(
                name: "--host",
                description: "Adresse du serveur SFTP") { IsRequired = true };

            var portOption = new Option<int>(
                name: "--port",
                description: "Port du serveur SFTP",
                getDefaultValue: () => 22);

            var usernameOption = new Option<string>(
                name: "--username",
                description: "Nom d'utilisateur") { IsRequired = true };

            var passwordOption = new Option<string>(
                name: "--password",
                description: "Mot de passe") { IsRequired = true };

            var remotePathOption = new Option<string>(
                name: "--remote-path",
                description: "Chemin distant à copier") { IsRequired = true };

            var localPathOption = new Option<string>(
                name: "--local-path",
                description: "Chemin local de destination") { IsRequired = true };

            var recursiveOption = new Option<bool>(
                name: "--recursive",
                description: "Copie récursive des dossiers",
                getDefaultValue: () => false);

            var logLevelOption = new Option<string>(
                name: "--log-level",
                description: "Niveau de logging (Verbose, Debug, Information, Warning, Error, Fatal)",
                getDefaultValue: () => "Information");

            var logFileOption = new Option<bool>(
                name: "--log-file",
                description: "Activer l'écriture des logs dans un fichier",
                getDefaultValue: () => true);

            var rootCommand = new RootCommand("Outil de copie SFTP vers local")
            {
                hostOption,
                portOption,
                usernameOption,
                passwordOption,
                remotePathOption,
                localPathOption,
                recursiveOption,
                logLevelOption,
                logFileOption
            };

            rootCommand.SetHandler(async (context) =>
            {
                var host = context.ParseResult.GetValueForOption(hostOption);
                var port = context.ParseResult.GetValueForOption(portOption);
                var username = context.ParseResult.GetValueForOption(usernameOption);
                var password = context.ParseResult.GetValueForOption(passwordOption);
                var remotePath = context.ParseResult.GetValueForOption(remotePathOption);
                var localPath = context.ParseResult.GetValueForOption(localPathOption);
                var recursive = context.ParseResult.GetValueForOption(recursiveOption);
                var logLevel = context.ParseResult.GetValueForOption(logLevelOption);
                var enableFileLogging = context.ParseResult.GetValueForOption(logFileOption);

                // Configuration dynamique de Serilog
                ConfigureSerilog(logLevel, enableFileLogging);

                // Recréer le loggerFactory avec le nouveau logger
                using var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                try
                {
                    var sftpLogger = loggerFactory.CreateLogger<SftpService>();
                    var sftpService = new SftpService(sftpLogger);
                    await sftpService.CopyFromSftpAsync(host, port, username, password, remotePath, localPath, recursive);
                }
                catch (Exception ex)
                {
                    Log.Fatal("💀 Erreur critique: {ErrorMessage}", ex.Message);
                    context.ExitCode = 1;
                }
            });

            return await rootCommand.InvokeAsync(args);
        }

        static void ConfigureSerilog(string logLevel, bool enableFileLogging)
        {
            // Parse du niveau de log
            var level = logLevel.ToLowerInvariant() switch
            {
                "verbose" => Serilog.Events.LogEventLevel.Verbose,
                "debug" => Serilog.Events.LogEventLevel.Debug,
                "information" => Serilog.Events.LogEventLevel.Information,
                "warning" => Serilog.Events.LogEventLevel.Warning,
                "error" => Serilog.Events.LogEventLevel.Error,
                "fatal" => Serilog.Events.LogEventLevel.Fatal,
                _ => Serilog.Events.LogEventLevel.Information
            };

            // Configuration du logger
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            if (enableFileLogging)
            {
                Directory.CreateDirectory("logs");
                configuration = configuration
                    .WriteTo.File("logs/sftp-copy-.txt", 
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 10,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            // Fermeture de l'ancien logger et création du nouveau
            Log.CloseAndFlush();
            Log.Logger = configuration.CreateLogger();
        }
    }
}
