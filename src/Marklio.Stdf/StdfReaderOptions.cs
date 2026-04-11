namespace Marklio.Stdf;

/// <summary>
/// Options for reading STDF files.
/// </summary>
public sealed class StdfReaderOptions
{
    /// <summary>
    /// When <c>true</c>, the reader attempts to recover from corrupted records
    /// by scanning forward for the next valid record header. When <c>false</c>
    /// (default), a corrupted record causes reading to stop.
    /// </summary>
    public bool RecoveryMode { get; set; }

    /// <summary>
    /// Optional callback invoked when recovery mode skips corrupted data.
    /// Receives the byte position in the stream and number of bytes skipped.
    /// </summary>
    public Action<StdfRecoveryEvent>? OnRecovery { get; set; }
}

/// <summary>
/// Information about a recovery event during STDF reading.
/// </summary>
public readonly struct StdfRecoveryEvent
{
    /// <summary>Approximate byte offset where corruption was detected.</summary>
    public long Position { get; init; }

    /// <summary>Number of bytes skipped to find the next valid record.</summary>
    public int BytesSkipped { get; init; }

    /// <summary>Description of why recovery was triggered.</summary>
    public string Reason { get; init; }
}
