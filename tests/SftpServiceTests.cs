using FluentAssertions;
using Microsoft.Extensions.Logging;
using SftpCopyTool;
using Xunit;

namespace SftpCopyTool.Tests;

/// <summary>Tests pour la classe SftpService.</summary>
public sealed class SftpServiceTests
{
    /// <summary>Teste que le constructeur lève une exception quand le logger est null.</summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new SftpService(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>Teste que le constructeur réussit avec un logger valide.</summary>
    [Fact]
    public void Constructor_WithValidLogger_Succeeds()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SftpService>();

        // Act
        var service = new SftpService(logger);

        // Assert
        service.Should().NotBeNull();
    }

    /// <summary>Teste la validation des paramètres de CopyFromSftpAsync.</summary>
    [Theory]
    [InlineData(null, "username", "password", "/remote", "/local")]
    [InlineData("", "username", "password", "/remote", "/local")]
    [InlineData("host", null, "password", "/remote", "/local")]
    [InlineData("host", "", "password", "/remote", "/local")]
    [InlineData("host", "username", null, "/remote", "/local")]
    [InlineData("host", "username", "", "/remote", "/local")]
    [InlineData("host", "username", "password", null, "/local")]
    [InlineData("host", "username", "password", "", "/local")]
    [InlineData("host", "username", "password", "/remote", null)]
    [InlineData("host", "username", "password", "/remote", "")]
    public async Task CopyFromSftpAsync_WithInvalidParameters_ThrowsArgumentException(
        string? host, string? username, string? password, string? remotePath, string? localPath)
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SftpService>();
        var service = new SftpService(logger);

        // Act & Assert
        var action = async () => await service.CopyFromSftpAsync(
            host!, 22, username!, password!, remotePath!, localPath!, false);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>Teste la validation du port négatif.</summary>
    [Fact]
    public async Task CopyFromSftpAsync_WithNegativePort_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SftpService>();
        var service = new SftpService(logger);

        // Act & Assert
        var action = async () => await service.CopyFromSftpAsync(
            "host", -1, "username", "password", "/remote", "/local", false);

        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
