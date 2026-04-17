using System.Buffers;

namespace Marklio.Stdf;

/// <summary>
/// A synthetic record injected into the record stream by validation extensions
/// to report spec violations, structural issues, or other problems.
/// </summary>
/// <remarks>
/// <para>
/// Error records use RecordType=0, RecordSubType=0, which are unused in
/// the STDF V4 specification and will not conflict with real records.
/// </para>
/// <para>
/// Error records cannot be serialized to an STDF file. Attempting to write
/// an <see cref="ErrorRecord"/> will throw <see cref="NotSupportedException"/>.
/// </para>
/// </remarks>
public sealed record class ErrorRecord : StdfRecord
{
    /// <inheritdoc/>
    public override byte RecordType => 0;

    /// <inheritdoc/>
    public override byte RecordSubType => 0;

    /// <summary>The severity of the issue.</summary>
    public required ErrorSeverity Severity { get; init; }

    /// <summary>Machine-readable error code (e.g. "STDF_ORDER_FAR_MISSING").</summary>
    public required string Code { get; init; }

    /// <summary>Human-readable description of the issue.</summary>
    public required string Message { get; init; }

    /// <summary>The original record that triggered the error, if applicable.</summary>
    public StdfRecord? SourceRecord { get; init; }

    /// <summary>Not supported — error records cannot be serialized.</summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    protected internal override void Serialize(IBufferWriter<byte> writer, Endianness endianness)
        => throw new NotSupportedException("ErrorRecord cannot be serialized to an STDF file.");
}
