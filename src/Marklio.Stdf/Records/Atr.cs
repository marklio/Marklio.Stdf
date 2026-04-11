using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// ATR — Audit Trail Record (0, 20).
/// Stores an audit trail message with a timestamp.
/// </summary>
[StdfRecord(0, 20)]
public partial record struct Atr
{
    /// <summary>Date and time of the audit trail entry.</summary>
    [StdfDateTime] public DateTime ModifiedTime { get; set; }

    /// <summary>Command line or description of the change.</summary>
    public string? CommandLine { get; set; }
}
