using System.Buffers;

namespace Marklio.Stdf;

/// <summary>
/// Represents an STDF record whose type is not recognized by the library.
/// Preserves raw record bytes for round-tripping.
/// </summary>
public readonly record struct UnknownRecord : IStdfRecord
{
    /// <summary>The STDF record type code.</summary>
    public required byte RecType { get; init; }

    /// <summary>The STDF record sub-type code.</summary>
    public required byte RecSub { get; init; }

    /// <summary>The raw record payload (excluding the 4-byte header).</summary>
    public required ReadOnlyMemory<byte> RawData { get; init; }

    // IStdfRecord: these are instance-level for unknown records since type/sub vary
    static byte IStdfRecord.RecordType => 0;
    static byte IStdfRecord.RecordSubType => 0;

    /// <summary>Writes the raw bytes directly to the buffer.</summary>
    public void Serialize(IBufferWriter<byte> writer, Endianness endianness)
    {
        if (RawData.Length > 0)
        {
            var span = writer.GetSpan(RawData.Length);
            RawData.Span.CopyTo(span);
            writer.Advance(RawData.Length);
        }
    }
}
