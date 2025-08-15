namespace SftpCopyTool;

using Microsoft.Extensions.Logging;

/// <summary>
///     Informations de progression pour les opérations de transfert de fichiers.
/// </summary>
/// <param name="BytesTransferred">Nombre d'octets transférés.</param>
/// <param name="TotalSize">Taille totale du fichier.</param>
/// <param name="FileName">Nom du fichier en cours de transfert.</param>
public record FileProgress(long BytesTransferred, long TotalSize, string FileName)
{
    /// <summary>Obtient le pourcentage de progression.</summary>
    public double PercentComplete => TotalSize > 0 ? (double)BytesTransferred / TotalSize * 100 : 0;

    /// <summary>Obtient la vitesse de transfert estimée (octets par seconde).</summary>
    public double Speed { get; init; }

    /// <summary>Obtient le temps restant estimé.</summary>
    public TimeSpan? EstimatedTimeRemaining { get; init; }
}

/// <summary>
///     Stream wrapper qui suit la progression des opérations d'écriture avec reporting périodique.
/// </summary>
internal sealed class ProgressTrackingStream : Stream
{
    private readonly Stream _baseStream;
    private readonly string _fileName;
    private readonly long _totalSize;
    private readonly ILogger _logger;
    private readonly DateTime _startTime;

    private long _totalBytesWritten;
    private double _lastReportedPercentage = -1;

    public ProgressTrackingStream(Stream baseStream, string fileName, long totalSize, ILogger logger)
    {
        _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
        _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        _totalSize = totalSize;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _startTime = DateTime.UtcNow;
    }

    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length => _baseStream.Length;
    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    public override void Flush() => _baseStream.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _baseStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _baseStream.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => _baseStream.ReadAsync(buffer, cancellationToken);

    public override void Write(byte[] buffer, int offset, int count)
    {
        _baseStream.Write(buffer, offset, count);
        ReportProgress(count);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await _baseStream.WriteAsync(buffer, offset, count, cancellationToken);
        ReportProgress(count);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await _baseStream.WriteAsync(buffer, cancellationToken);
        ReportProgress(buffer.Length);
    }

    private void ReportProgress(int bytesWritten)
    {
        _totalBytesWritten += bytesWritten;
        var percentage = _totalSize > 0 ? (double)_totalBytesWritten / _totalSize * 100 : 0;

        // Reporter seulement tous les 5% pour éviter le spam de logs
        if (percentage - _lastReportedPercentage >= 5.0 || percentage >= 100.0)
        {
            var elapsed = DateTime.UtcNow - _startTime;
            var speed = elapsed.TotalSeconds > 0 ? _totalBytesWritten / elapsed.TotalSeconds : 0;

            _logger.LogInformation("📊 {FileName}: {Percentage:F1}% ({Transferred}/{Total}) - {Speed}/s",
                _fileName, percentage, FormatBytes(_totalBytesWritten), FormatBytes(_totalSize), FormatBytes((long)speed));

            _lastReportedPercentage = percentage;
        }
    }

    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
    public override void SetLength(long value) => _baseStream.SetLength(value);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _baseStream?.Dispose();
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_baseStream != null)
        {
            await _baseStream.DisposeAsync();
        }
        await base.DisposeAsync();
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
