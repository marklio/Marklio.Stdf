using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace Marklio.Stdf.IO;

/// <summary>
/// Reads STDF records from a <see cref="PipeReader"/>, yielding them
/// as an <see cref="IAsyncEnumerable{StdfRecord}"/>.
/// </summary>
internal sealed class StdfRecordReader
{
    private readonly PipeReader _pipeReader;
    private readonly StdfReaderOptions _options;
    private Endianness _endianness = Endianness.LittleEndian;
    private bool _endiannessDetected;
    private long _bytesConsumed;

    public StdfRecordReader(PipeReader pipeReader, StdfReaderOptions? options = null)
    {
        _pipeReader = pipeReader;
        _options = options ?? new StdfReaderOptions();
    }

    /// <summary>
    /// Reads all STDF records from the pipe.
    /// Endianness is auto-detected from the FAR record.
    /// The caller is responsible for calling <see cref="PipeReader.CompleteAsync"/>
    /// after enumeration finishes or is abandoned.
    /// </summary>
    public async IAsyncEnumerable<StdfRecord> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const int HeaderSize = 4; // REC_LEN (U*2) + REC_TYP (U*1) + REC_SUB (U*1)

        byte[] hdr = new byte[4];

        while (true)
        {
            var readResult = await _pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var buffer = readResult.Buffer;

            while (true)
            {
                if (buffer.Length < HeaderSize)
                {
                    if (readResult.IsCompleted)
                        yield break;
                    break; // need more data
                }

                // For FAR (first record), peek at CPU_TYP to detect endianness
                // before interpreting REC_LEN. CPU_TYP is the first payload byte (offset 4).
                if (!_endiannessDetected && buffer.Length >= HeaderSize + 1)
                {
                    var peekReader = new SequenceReader<byte>(buffer);
                    peekReader.Advance(4); // skip header
                    peekReader.TryRead(out byte cpuType);
                    _endianness = cpuType == 1 ? Endianness.BigEndian : Endianness.LittleEndian;
                    _endiannessDetected = true;
                }

                // Read the 4-byte header
                var headerReader = new SequenceReader<byte>(buffer);
                headerReader.TryCopyTo(hdr);

                ushort recLen = _endianness == Endianness.LittleEndian
                    ? System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(hdr)
                    : System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(hdr);
                byte recType = hdr[2];
                byte recSub = hdr[3];

                long totalRecordSize = HeaderSize + recLen;
                if (buffer.Length < totalRecordSize)
                {
                    if (readResult.IsCompleted)
                    {
                        if (_options.RecoveryMode)
                        {
                            // Truncated record — try scanning ahead
                            var skipped = TryScanForNextRecord(ref buffer, readResult.IsCompleted);
                            if (skipped < 0) yield break;
                            continue;
                        }
                        yield break; // truncated record at end of stream
                    }
                    break; // need more data
                }

                // Slice out just the record payload
                var payloadSlice = buffer.Slice(HeaderSize, recLen);
                var payloadReader = new SequenceReader<byte>(payloadSlice);

                StdfRecord? yielded = null;
                try
                {
                    // Dispatch to generated deserializer
                    var record = RecordRegistry.Deserialize(recType, recSub, ref payloadReader, _endianness);
                    if (record.HasValue)
                    {
                        // Capture any trailing bytes not consumed by the deserializer
                        ReadOnlyMemory<byte> trailing = default;
                        if (payloadReader.Remaining > 0)
                        {
                            var trailingBytes = new byte[payloadReader.Remaining];
                            payloadReader.TryCopyTo(trailingBytes);
                            trailing = trailingBytes;
                        }

                        yielded = new StdfRecord(record.Value.Record, recType, recSub, trailing);
                    }
                    else
                    {
                        // Unknown record — preserve raw bytes
                        var rawData = payloadSlice.ToArray();
                        var unknown = new UnknownRecord { RecType = recType, RecSub = recSub, RawData = rawData };
                        yielded = new StdfRecord(unknown, recType, recSub);
                    }
                }
                catch when (_options.RecoveryMode)
                {
                    // Deserialization failed — skip this record and report recovery
                    _options.OnRecovery?.Invoke(new StdfRecoveryEvent
                    {
                        Position = _bytesConsumed,
                        BytesSkipped = (int)totalRecordSize,
                        Reason = $"Deserialization failed for record ({recType},{recSub}) with length {recLen}"
                    });
                }

                _bytesConsumed += totalRecordSize;
                buffer = buffer.Slice(totalRecordSize);

                if (yielded.HasValue)
                    yield return yielded.Value;
            }

            _pipeReader.AdvanceTo(buffer.Start, buffer.End);

            if (readResult.IsCompleted)
                break;
        }
    }

    /// <summary>
    /// Scans forward in the buffer looking for a valid record header.
    /// Returns the number of bytes skipped, or -1 if no valid header was found.
    /// </summary>
    private int TryScanForNextRecord(ref ReadOnlySequence<byte> buffer, bool isComplete)
    {
        const int HeaderSize = 4;
        if (buffer.Length < HeaderSize + 1) return -1;

        var scanReader = new SequenceReader<byte>(buffer);
        scanReader.Advance(1); // skip at least one byte
        int skipped = 1;

        Span<byte> probe = stackalloc byte[4];
        Span<byte> nextProbe = stackalloc byte[4];

        while (scanReader.Remaining >= HeaderSize)
        {
            scanReader.TryCopyTo(probe);
            byte probType = probe[2];
            byte probSub = probe[3];

            if (RecordRegistry.IsKnown(probType, probSub))
            {
                ushort probLen = _endianness == Endianness.LittleEndian
                    ? System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(probe)
                    : System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(probe);

                // Sanity: if we can see the next record after this candidate, validate it too
                long candidateTotal = HeaderSize + probLen;
                if (scanReader.Remaining >= candidateTotal + HeaderSize)
                {
                    var savePos = scanReader;
                    savePos.Advance(candidateTotal);
                    savePos.TryCopyTo(nextProbe);
                    byte nextType = nextProbe[2];
                    byte nextSub = nextProbe[3];
                    if (RecordRegistry.IsKnown(nextType, nextSub))
                    {
                        // Double-validated: this is likely a real record
                        _options.OnRecovery?.Invoke(new StdfRecoveryEvent
                        {
                            Position = _bytesConsumed,
                            BytesSkipped = skipped,
                            Reason = $"Scanned past {skipped} corrupt bytes to find record ({probType},{probSub})"
                        });
                        _bytesConsumed += skipped;
                        buffer = buffer.Slice(skipped);
                        return skipped;
                    }
                }
                else if (isComplete && scanReader.Remaining >= candidateTotal)
                {
                    // Last record in file — accept single validation
                    _options.OnRecovery?.Invoke(new StdfRecoveryEvent
                    {
                        Position = _bytesConsumed,
                        BytesSkipped = skipped,
                        Reason = $"Scanned past {skipped} corrupt bytes to find record ({probType},{probSub})"
                    });
                    _bytesConsumed += skipped;
                    buffer = buffer.Slice(skipped);
                    return skipped;
                }
            }

            scanReader.Advance(1);
            skipped++;
        }

        return -1; // no valid header found
    }
}
