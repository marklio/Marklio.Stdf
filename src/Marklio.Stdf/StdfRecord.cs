namespace Marklio.Stdf;

/// <summary>
/// Wrapper that holds any STDF record type. Used as the element type
/// for <c>IAsyncEnumerable&lt;StdfRecord&gt;</c>.
/// Supports pattern matching via <c>is</c> / <c>switch</c>.
/// </summary>
public readonly struct StdfRecord
{
    private readonly IStdfRecord _record;

    /// <summary>The underlying record instance.</summary>
    public IStdfRecord Record => _record;

    /// <summary>The record type code from the STDF header.</summary>
    public byte RecordType { get; }

    /// <summary>The record sub-type code from the STDF header.</summary>
    public byte RecordSubType { get; }

    /// <summary>
    /// Bytes trailing after the last recognized field in the record payload.
    /// Preserved for byte-exact round-tripping of padding or vendor extensions.
    /// </summary>
    public ReadOnlyMemory<byte> TrailingData { get; }

    internal StdfRecord(IStdfRecord record, byte recType, byte recSub, ReadOnlyMemory<byte> trailingData = default)
    {
        _record = record;
        RecordType = recType;
        RecordSubType = recSub;
        TrailingData = trailingData;
    }

    /// <summary>
    /// Attempts to cast the underlying record to <typeparamref name="T"/>.
    /// </summary>
    public bool Is<T>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? value) where T : IStdfRecord
    {
        if (_record is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Returns the underlying record if it is of type <typeparamref name="T"/>,
    /// or <c>null</c> otherwise.
    /// </summary>
    public T? As<T>() where T : struct, IStdfRecord
    {
        return _record is T typed ? typed : null;
    }
}
