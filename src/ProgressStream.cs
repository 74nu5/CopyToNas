namespace SftpCopyTool;

/// <summary>
///     Informations de progression pour les opérations de transfert de fichiers.
/// </summary>
/// <param name="BytesTransferred">Nombre d'octets transférés.</param>
/// <param name="TotalSize">Taille totale du fichier.</param>
public record ProgressInfo(long BytesTransferred, long TotalSize)
{
    /// <summary>Obtient le pourcentage de progression.</summary>
    public double PercentComplete => TotalSize > 0 ? (double)BytesTransferred / TotalSize * 100 : 0;
}

/// <summary>
///     Stream wrapper qui suit la progression des opérations d'écriture.
/// </summary>
public sealed class ProgressStream : Stream
{
    private readonly Stream _baseStream;
    private readonly long _totalSize;
    private readonly IProgress<ProgressInfo> _progress;
    private long _totalBytesWritten;

    /// <summary>Initialise une nouvelle instance de la classe <see cref="ProgressStream"/>.</summary>
    /// <param name="baseStream">Stream de base à wrapper.</param>
    /// <param name="totalSize">Taille totale attendue.</param>
    /// <param name="progress">Interface de rapport de progression.</param>
    /// <exception cref="ArgumentNullException">Levée quand baseStream est null.</exception>
    public ProgressStream(Stream baseStream, long totalSize, IProgress<ProgressInfo> progress)
    {
        _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
        _totalSize = totalSize;
        _progress = progress;
    }

    /// <summary>Obtient une valeur indiquant si le stream peut être lu.</summary>
    public override bool CanRead => _baseStream.CanRead;

    /// <summary>Obtient une valeur indiquant si le stream peut être navigué.</summary>
    public override bool CanSeek => _baseStream.CanSeek;

    /// <summary>Obtient une valeur indiquant si le stream peut être écrit.</summary>
    public override bool CanWrite => _baseStream.CanWrite;

    /// <summary>Obtient la longueur du stream.</summary>
    public override long Length => _baseStream.Length;

    /// <summary>Obtient ou définit la position dans le stream.</summary>
    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    /// <summary>Vide tous les tampons pour ce stream.</summary>
    public override void Flush() => _baseStream.Flush();

    /// <summary>Vide de manière asynchrone tous les tampons pour ce stream.</summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    public override Task FlushAsync(CancellationToken cancellationToken) => _baseStream.FlushAsync(cancellationToken);

    /// <summary>Lit des données depuis le stream.</summary>
    /// <param name="buffer">Buffer de destination.</param>
    /// <param name="offset">Offset dans le buffer.</param>
    /// <param name="count">Nombre d'octets à lire.</param>
    /// <returns>Nombre d'octets lus.</returns>
    public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);

    /// <summary>Lit des données de manière asynchrone depuis le stream.</summary>
    /// <param name="buffer">Buffer de destination.</param>
    /// <param name="offset">Offset dans le buffer.</param>
    /// <param name="count">Nombre d'octets à lire.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche contenant le nombre d'octets lus.</returns>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var bytesRead = await _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
        return bytesRead;
    }

    /// <summary>Lit des données de manière asynchrone depuis le stream.</summary>
    /// <param name="buffer">Buffer de destination.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche contenant le nombre d'octets lus.</returns>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var bytesRead = await _baseStream.ReadAsync(buffer, cancellationToken);
        return bytesRead;
    }

    /// <summary>Écrit des données dans le stream.</summary>
    /// <param name="buffer">Buffer source.</param>
    /// <param name="offset">Offset dans le buffer.</param>
    /// <param name="count">Nombre d'octets à écrire.</param>
    public override void Write(byte[] buffer, int offset, int count)
    {
        _baseStream.Write(buffer, offset, count);
        ReportProgress(count);
    }

    /// <summary>Écrit des données de manière asynchrone dans le stream.</summary>
    /// <param name="buffer">Buffer source.</param>
    /// <param name="offset">Offset dans le buffer.</param>
    /// <param name="count">Nombre d'octets à écrire.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await _baseStream.WriteAsync(buffer, offset, count, cancellationToken);
        ReportProgress(count);
    }

    /// <summary>Écrit des données de manière asynchrone dans le stream.</summary>
    /// <param name="buffer">Buffer source.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await _baseStream.WriteAsync(buffer, cancellationToken);
        ReportProgress(buffer.Length);
    }

    private void ReportProgress(int bytesWritten)
    {
        _totalBytesWritten += bytesWritten;
        _progress?.Report(new ProgressInfo(_totalBytesWritten, _totalSize));
    }

    /// <summary>Définit la position dans le stream.</summary>
    /// <param name="offset">Offset par rapport à l'origine.</param>
    /// <param name="origin">Point de référence.</param>
    /// <returns>Nouvelle position dans le stream.</returns>
    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

    /// <summary>Définit la longueur du stream.</summary>
    /// <param name="value">Nouvelle longueur.</param>
    public override void SetLength(long value) => _baseStream.SetLength(value);

    /// <summary>Libère les ressources utilisées par le stream.</summary>
    /// <param name="disposing">Indique si les ressources managées doivent être libérées.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _baseStream?.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>Libère de manière asynchrone les ressources utilisées par le stream.</summary>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    public override async ValueTask DisposeAsync()
    {
        if (_baseStream != null)
        {
            await _baseStream.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}
