using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// BPS — Begin Program Section Record (20, 10).
/// Marks the beginning of a new program section.
/// </summary>
[StdfRecord(20, 10)]
public partial record struct Bps
{
    /// <summary>Program section name.</summary>
    public string? SequenceName { get; set; }
}
