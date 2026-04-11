using System.Buffers;

namespace Marklio.Stdf;

/// <summary>
/// Common interface for all STDF record types.
/// Implemented by source-generated partial record structs.
/// </summary>
public interface IStdfRecord
{
    /// <summary>STDF record type code.</summary>
    static abstract byte RecordType { get; }

    /// <summary>STDF record sub-type code.</summary>
    static abstract byte RecordSubType { get; }

    /// <summary>Serializes this record's payload to the given buffer writer.</summary>
    void Serialize(IBufferWriter<byte> writer, Endianness endianness);
}
