using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// BPS — Begin Program Section Record (20, 10).
/// Marks the beginning of a named program section.
/// </summary>
[StdfRecord(20, 10)]
public partial record class Bps
{
    /// <summary>
    /// Name or ID of the program section.
    /// [STDF: SEQ_NAME, C*n]
    /// </summary>
    public string? SequenceName { get; set; }
}
