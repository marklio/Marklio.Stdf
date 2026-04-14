using System.Buffers;

namespace Marklio.Stdf;

/// <summary>
/// Represents an STDF record whose type is not recognized by the library.
/// Preserves raw record bytes for round-tripping.
/// </summary>
public sealed record class UnknownRecord : StdfRecord
{
    /// <summary>The STDF record type code.</summary>
    public override byte RecordType { get; }

    /// <summary>The STDF record sub-type code.</summary>
    public override byte RecordSubType { get; }

    /// <summary>The raw record payload (excluding the 4-byte header).</summary>
    public required ReadOnlyMemory<byte> RawData { get; init; }

    /// <summary>
    /// Creates an unknown record with the specified type and sub-type codes.
    /// </summary>
    public UnknownRecord(byte recordType, byte recordSubType)
    {
        RecordType = recordType;
        RecordSubType = recordSubType;
    }

    /// <summary>Writes the raw bytes directly to the buffer.</summary>
    protected internal override void Serialize(IBufferWriter<byte> writer, Endianness endianness)
    {
        if (RawData.Length > 0)
        {
            var span = writer.GetSpan(RawData.Length);
            RawData.Span.CopyTo(span);
            writer.Advance(RawData.Length);
        }
    }
}
