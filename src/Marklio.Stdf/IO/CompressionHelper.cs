using System.IO.Compression;
using ICSharpCode.SharpZipLib.BZip2;

namespace Marklio.Stdf.IO;

/// <summary>
/// Detects and handles gzip/bzip2 compression for STDF streams.
/// </summary>
internal static class CompressionHelper
{
    // Gzip magic: 1F 8B
    // Bzip2 magic: 42 5A (BZ)
    private const byte GzipMagic1 = 0x1F;
    private const byte GzipMagic2 = 0x8B;
    private const byte Bzip2Magic1 = 0x42; // 'B'
    private const byte Bzip2Magic2 = 0x5A; // 'Z'

    /// <summary>
    /// Detects compression from magic bytes at the start of a seekable stream.
    /// Resets the stream position afterward.
    /// Non-seekable streams return <see cref="StdfCompression.None"/> because
    /// length/position are unavailable; callers must pre-decompress or specify
    /// compression explicitly.
    /// </summary>
    public static StdfCompression Detect(Stream stream)
    {
        if (!stream.CanRead || !stream.CanSeek)
            return StdfCompression.None;

        if (stream.Length < 2)
            return StdfCompression.None;

        long pos = stream.Position;
        Span<byte> magic = stackalloc byte[2];
        int read = stream.Read(magic);
        stream.Position = pos;

        if (read < 2)
            return StdfCompression.None;

        if (magic[0] == GzipMagic1 && magic[1] == GzipMagic2)
            return StdfCompression.Gzip;

        if (magic[0] == Bzip2Magic1 && magic[1] == Bzip2Magic2)
            return StdfCompression.Bzip2;

        return StdfCompression.None;
    }

    /// <summary>
    /// Detects compression from magic bytes in a byte span.
    /// </summary>
    public static StdfCompression Detect(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return StdfCompression.None;

        if (data[0] == GzipMagic1 && data[1] == GzipMagic2)
            return StdfCompression.Gzip;

        if (data[0] == Bzip2Magic1 && data[1] == Bzip2Magic2)
            return StdfCompression.Bzip2;

        return StdfCompression.None;
    }

    /// <summary>
    /// Wraps a read stream with decompression if compression is detected.
    /// Returns the original stream if uncompressed.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="leaveOpen">
    /// If <c>true</c>, the inner <paramref name="stream"/> is not closed
    /// when the decompression wrapper is disposed.
    /// </param>
    public static Stream WrapForReading(Stream stream, bool leaveOpen = false)
    {
        var compression = Detect(stream);
        return compression switch
        {
            StdfCompression.Gzip => new GZipStream(stream, CompressionMode.Decompress, leaveOpen: leaveOpen),
            StdfCompression.Bzip2 => new BZip2InputStream(stream) { IsStreamOwner = !leaveOpen },
            _ => stream,
        };
    }

    /// <summary>
    /// Decompresses a byte buffer if it starts with compression magic bytes.
    /// Returns the original data if uncompressed.
    /// </summary>
    public static ReadOnlyMemory<byte> DecompressIfNeeded(ReadOnlyMemory<byte> data)
    {
        var compression = Detect(data.Span);
        if (compression == StdfCompression.None)
            return data;

        using var input = new MemoryStream(data.ToArray(), writable: false);
        using var decompressed = new MemoryStream();

        using (var decompressionStream = compression switch
        {
            StdfCompression.Gzip => (Stream)new GZipStream(input, CompressionMode.Decompress),
            StdfCompression.Bzip2 => new BZip2InputStream(input) { IsStreamOwner = false },
            _ => throw new InvalidOperationException(),
        })
        {
            decompressionStream.CopyTo(decompressed);
        }

        return decompressed.ToArray();
    }

    /// <summary>
    /// Wraps a write stream with compression.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="compression">The compression algorithm to use.</param>
    /// <param name="leaveOpen">
    /// If <c>true</c>, the inner <paramref name="stream"/> is not closed
    /// when the compression wrapper is disposed.
    /// </param>
    public static Stream WrapForWriting(Stream stream, StdfCompression compression, bool leaveOpen = false)
    {
        return compression switch
        {
            StdfCompression.Gzip => new GZipStream(stream, CompressionLevel.Optimal, leaveOpen: leaveOpen),
            StdfCompression.Bzip2 => new BZip2OutputStream(stream) { IsStreamOwner = !leaveOpen },
            _ => stream,
        };
    }
}
