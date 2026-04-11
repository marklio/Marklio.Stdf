namespace Marklio.Stdf;

/// <summary>
/// Wrapper that holds any STDF record type. Used as the element type
/// for <c>IAsyncEnumerable&lt;StdfRecord&gt;</c>.
/// Use <see cref="TryGetRecord{T}(out T)"/> to unwrap to a specific record type,
/// or pattern-match on <see cref="Record"/> via <c>is</c> / <c>switch</c>.
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
    /// Attempts to retrieve the record as the specified type.
    /// </summary>
    /// <typeparam name="T">The STDF record type.</typeparam>
    /// <param name="record">When this method returns <c>true</c>, contains the typed record.</param>
    /// <returns><c>true</c> if the record is of type <typeparamref name="T"/>; otherwise, <c>false</c>.</returns>
    public bool TryGetRecord<T>(out T record) where T : struct, IStdfRecord
    {
        if (_record is T typed)
        {
            record = typed;
            return true;
        }
        record = default;
        return false;
    }

    /// <summary>
    /// Attempts to cast the underlying record to <typeparamref name="T"/>.
    /// Unlike <see cref="TryGetRecord{T}(out T)"/>, this method also accepts
    /// interface types such as <c>ITestRecord</c> or <c>IBinRecord</c>.
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
    /// Returns the underlying record as <typeparamref name="T"/> if it matches,
    /// or <c>null</c> otherwise. Prefer <see cref="TryGetRecord{T}(out T)"/>
    /// for the standard try-pattern.
    /// </summary>
    public T? As<T>() where T : struct, IStdfRecord
    {
        return _record is T typed ? typed : null;
    }
}
