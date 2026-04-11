using System.Buffers;
using System.IO.Pipelines;

namespace Marklio.Stdf.IO;

/// <summary>
/// Writes STDF records to a <see cref="PipeWriter"/>.
/// </summary>
internal sealed class StdfRecordWriter : IAsyncDisposable
{
    private readonly PipeWriter _pipeWriter;
    private readonly Endianness _endianness;

    public StdfRecordWriter(PipeWriter pipeWriter, Endianness endianness)
    {
        _pipeWriter = pipeWriter;
        _endianness = endianness;
    }

    /// <summary>
    /// Writes a single STDF record (header + payload).
    /// </summary>
    public async ValueTask WriteAsync(StdfRecord record, CancellationToken cancellationToken = default)
    {
        // Serialize payload to a temporary buffer to compute REC_LEN
        var payloadBuffer = new ArrayBufferWriter<byte>();
        record.Record.Serialize(payloadBuffer, _endianness);

        // Include trailing data for byte-exact round-tripping
        int trailingLen = record.TrailingData.Length;
        int totalPayload = payloadBuffer.WrittenCount + trailingLen;

        // Write 4-byte header: REC_LEN (U*2) + REC_TYP (U*1) + REC_SUB (U*1)
        if (totalPayload > ushort.MaxValue)
            throw new InvalidOperationException($"Record payload size {totalPayload} exceeds maximum STDF record length of {ushort.MaxValue} bytes.");
        var recLen = (ushort)totalPayload;
        var headerSpan = _pipeWriter.GetSpan(4);
        if (_endianness == Endianness.LittleEndian)
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(headerSpan, recLen);
        else
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(headerSpan, recLen);
        headerSpan[2] = record.RecordType;
        headerSpan[3] = record.RecordSubType;
        _pipeWriter.Advance(4);

        // Write payload
        if (payloadBuffer.WrittenCount > 0)
        {
            var payloadSpan = _pipeWriter.GetSpan(payloadBuffer.WrittenCount);
            payloadBuffer.WrittenSpan.CopyTo(payloadSpan);
            _pipeWriter.Advance(payloadBuffer.WrittenCount);
        }

        // Write trailing data
        if (trailingLen > 0)
        {
            var trailingSpan = _pipeWriter.GetSpan(trailingLen);
            record.TrailingData.Span.CopyTo(trailingSpan);
            _pipeWriter.Advance(trailingLen);
        }

        var flushResult = await _pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        if (flushResult.IsCanceled)
            throw new OperationCanceledException();
    }

    /// <summary>Completes the underlying pipe writer.</summary>
    public async ValueTask DisposeAsync()
    {
        await _pipeWriter.CompleteAsync().ConfigureAwait(false);
    }
}
