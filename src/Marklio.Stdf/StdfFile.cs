using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Marklio.Stdf.IO;

namespace Marklio.Stdf;

/// <summary>
/// Static entry points for reading and writing STDF files.
/// </summary>
public static class StdfFile
{
    /// <summary>
    /// Reads STDF records from a file path.
    /// </summary>
    public static IAsyncEnumerable<StdfRecord> ReadAsync(
        string path,
        StdfReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var stream = File.OpenRead(path);
        return ReadAsyncCore(stream, ownsStream: true, options, cancellationToken);
    }

    /// <summary>
    /// Reads STDF records from a <see cref="Stream"/>.
    /// The caller retains ownership of the stream.
    /// </summary>
    public static IAsyncEnumerable<StdfRecord> ReadAsync(
        Stream stream,
        StdfReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return ReadAsyncCore(stream, ownsStream: false, options, cancellationToken);
    }

    /// <summary>
    /// Reads STDF records synchronously from a <see cref="ReadOnlyMemory{T}"/>.
    /// Automatically detects and decompresses gzip/bzip2 data.
    /// </summary>
    public static IEnumerable<StdfRecord> Read(ReadOnlyMemory<byte> data, StdfReaderOptions? options = null)
    {
        data = IO.CompressionHelper.DecompressIfNeeded(data);
        var sequence = new ReadOnlySequence<byte>(data);
        return ReadFromSequence(sequence, options);
    }

    /// <summary>
    /// Opens a writer for producing an STDF file at the given path.
    /// </summary>
    public static async Task<StdfWriter> OpenWriteAsync(
        string path,
        StdfWriterOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var opts = options ?? new StdfWriterOptions();
        var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 65536, useAsync: true);
        Stream writeStream = opts.Compression != StdfCompression.None
            ? IO.CompressionHelper.WrapForWriting(fileStream, opts.Compression, leaveOpen: true)
            : fileStream;
        var pipeWriter = PipeWriter.Create(writeStream, new StreamPipeWriterOptions(leaveOpen: true));
        return new StdfWriter(new StdfRecordWriter(pipeWriter, opts.Endianness), fileStream,
            compressionStream: !ReferenceEquals(writeStream, fileStream) ? writeStream : null);
    }

    /// <summary>
    /// Opens a writer for producing STDF data to a stream.
    /// </summary>
    public static StdfWriter OpenWrite(
        Stream stream,
        StdfWriterOptions? options = null)
    {
        var opts = options ?? new StdfWriterOptions();
        Stream writeStream = opts.Compression != StdfCompression.None
            ? IO.CompressionHelper.WrapForWriting(stream, opts.Compression, leaveOpen: true)
            : stream;
        var pipeWriter = PipeWriter.Create(writeStream, new StreamPipeWriterOptions(leaveOpen: true));
        return new StdfWriter(new StdfRecordWriter(pipeWriter, opts.Endianness), ownsStream: null,
            compressionStream: !ReferenceEquals(writeStream, stream) ? writeStream : null);
    }

    private static async IAsyncEnumerable<StdfRecord> ReadAsyncCore(
        Stream stream,
        bool ownsStream,
        StdfReaderOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Detect and wrap compressed streams.
        // When we don't own the stream, leaveOpen:true prevents the wrapper from closing it.
        var readStream = IO.CompressionHelper.WrapForReading(stream, leaveOpen: !ownsStream);
        bool wrappedStream = !ReferenceEquals(readStream, stream);

        // PipeReader should only leave the underlying stream open when it IS the
        // caller's stream (not wrapped) and we don't own it.
        var pipeReader = PipeReader.Create(readStream, new StreamPipeReaderOptions(
            leaveOpen: !ownsStream && !wrappedStream));
        var reader = new StdfRecordReader(pipeReader, options);

        try
        {
            await foreach (var record in reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                yield return record;
            }
        }
        finally
        {
            await pipeReader.CompleteAsync().ConfigureAwait(false);
        }
    }

    private static IEnumerable<StdfRecord> ReadFromSequence(ReadOnlySequence<byte> sequence, StdfReaderOptions? options = null)
    {
        var opts = options ?? new StdfReaderOptions();
        var records = new List<StdfRecord>();
        var endianness = Endianness.LittleEndian;
        var endiannessDetected = false;
        var reader = new SequenceReader<byte>(sequence);

        Span<byte> hdr = stackalloc byte[4];
        while (reader.Remaining >= 4)
        {
            reader.TryCopyTo(hdr);

            // For FAR (first record), peek at CPU_TYP to detect endianness
            if (!endiannessDetected && reader.Remaining >= 5)
            {
                var saved = reader;
                saved.Advance(4);
                saved.TryRead(out byte cpuType);
                endianness = cpuType == 1 ? Endianness.BigEndian : Endianness.LittleEndian;
                endiannessDetected = true;
            }

            reader.Advance(4);

            ushort recLen = endianness == Endianness.LittleEndian
                ? System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(hdr)
                : System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(hdr);
            byte recType = hdr[2];
            byte recSub = hdr[3];

            if (reader.Remaining < recLen)
            {
                if (opts.RecoveryMode)
                {
                    // Truncated record — try scanning for next valid header
                    reader.Rewind(4); // back up to before the header
                    if (!SyncScanForNextRecord(ref reader, endianness, sequence, opts))
                        break;
                    continue;
                }
                break; // truncated
            }

            long payloadStart = reader.Consumed;
            var payloadSlice = sequence.Slice(payloadStart, recLen);
            var payloadReader = new SequenceReader<byte>(payloadSlice);

            try
            {
                var record = IO.RecordRegistry.Deserialize(recType, recSub, ref payloadReader, endianness);
                if (record.HasValue)
                {
                    ReadOnlyMemory<byte> trailing = default;
                    if (payloadReader.Remaining > 0)
                    {
                        var trailingBytes = new byte[payloadReader.Remaining];
                        payloadReader.TryCopyTo(trailingBytes);
                        trailing = trailingBytes;
                    }

                    records.Add(new StdfRecord(record.Value.Record, recType, recSub, trailing));
                }
                else
                {
                    var rawBytes = payloadSlice.ToArray();
                    records.Add(new StdfRecord(
                        new UnknownRecord { RecType = recType, RecSub = recSub, RawData = rawBytes },
                        recType, recSub));
                }
            }
            catch when (opts.RecoveryMode)
            {
                opts.OnRecovery?.Invoke(new StdfRecoveryEvent
                {
                    Position = reader.Consumed - 4,
                    BytesSkipped = 4 + recLen,
                    Reason = $"Deserialization failed for record ({recType},{recSub}) with length {recLen}"
                });
            }

            reader.Advance(recLen);
        }

        return records;
    }

    private static bool SyncScanForNextRecord(
        ref SequenceReader<byte> reader,
        Endianness endianness,
        ReadOnlySequence<byte> sequence,
        StdfReaderOptions opts)
    {
        long scanStart = reader.Consumed;
        reader.Advance(1); // skip at least one byte

        Span<byte> probe = stackalloc byte[4];
        Span<byte> nextProbe = stackalloc byte[4];
        while (reader.Remaining >= 4)
        {
            reader.TryCopyTo(probe);
            byte probType = probe[2];
            byte probSub = probe[3];

            if (IO.RecordRegistry.IsKnown(probType, probSub))
            {
                ushort probLen = endianness == Endianness.LittleEndian
                    ? System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(probe)
                    : System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(probe);

                long candidateTotal = 4 + probLen;
                // Double-validate: check if the record after this candidate also looks valid
                if (reader.Remaining >= candidateTotal + 4)
                {
                    var savePos = reader;
                    savePos.Advance(candidateTotal);
                    savePos.TryCopyTo(nextProbe);
                    if (IO.RecordRegistry.IsKnown(nextProbe[2], nextProbe[3]))
                    {
                        int skipped = (int)(reader.Consumed - scanStart);
                        opts.OnRecovery?.Invoke(new StdfRecoveryEvent
                        {
                            Position = scanStart,
                            BytesSkipped = skipped,
                            Reason = $"Scanned past {skipped} corrupt bytes to find record ({probType},{probSub})"
                        });
                        return true;
                    }
                }
                else if (reader.Remaining >= candidateTotal)
                {
                    // Last record in data — accept single validation
                    int skipped = (int)(reader.Consumed - scanStart);
                    opts.OnRecovery?.Invoke(new StdfRecoveryEvent
                    {
                        Position = scanStart,
                        BytesSkipped = skipped,
                        Reason = $"Scanned past {skipped} corrupt bytes to find record ({probType},{probSub})"
                    });
                    return true;
                }
            }

            reader.Advance(1);
        }

        return false; // no valid header found
    }
}

/// <summary>
/// Writes STDF records to a stream. Dispose when complete.
/// </summary>
public sealed class StdfWriter : IAsyncDisposable
{
    private readonly StdfRecordWriter _writer;
    private readonly Stream? _ownedStream;
    private readonly Stream? _compressionStream;

    internal StdfWriter(StdfRecordWriter writer, Stream? ownsStream, Stream? compressionStream = null)
    {
        _writer = writer;
        _ownedStream = ownsStream;
        _compressionStream = compressionStream;
    }

    /// <summary>Writes a single STDF record.</summary>
    public ValueTask WriteAsync(StdfRecord record, CancellationToken cancellationToken = default)
        => _writer.WriteAsync(record, cancellationToken);

    /// <summary>
    /// Helper to write a typed record struct, wrapping it in a StdfRecord automatically.
    /// </summary>
    public ValueTask WriteAsync<T>(T record, CancellationToken cancellationToken = default) where T : struct, IStdfRecord
        => _writer.WriteAsync(new StdfRecord(record, T.RecordType, T.RecordSubType), cancellationToken);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync().ConfigureAwait(false);
        if (_compressionStream != null)
            await _compressionStream.DisposeAsync().ConfigureAwait(false);
        if (_ownedStream != null)
            await _ownedStream.DisposeAsync().ConfigureAwait(false);
    }
}
