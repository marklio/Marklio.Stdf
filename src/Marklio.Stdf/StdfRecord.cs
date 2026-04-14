using System.Buffers;

namespace Marklio.Stdf;

/// <summary>
/// Abstract base class for all STDF record types. Used as the element type
/// for <c>IAsyncEnumerable&lt;StdfRecord&gt;</c>.
/// Pattern-match on concrete types via <c>is</c> / <c>switch</c>.
/// </summary>
public abstract record class StdfRecord
{
    /// <summary>The record type code from the STDF header.</summary>
    public abstract byte RecordType { get; }

    /// <summary>The record sub-type code from the STDF header.</summary>
    public abstract byte RecordSubType { get; }

    /// <summary>
    /// Bytes trailing after the last recognized field in the record payload.
    /// Preserved for byte-exact round-tripping of padding or vendor extensions.
    /// </summary>
    public ReadOnlyMemory<byte> TrailingData { get; internal set; }

    /// <summary>Serializes this record's payload to the given buffer writer.</summary>
    protected internal abstract void Serialize(IBufferWriter<byte> writer, Endianness endianness);
}
