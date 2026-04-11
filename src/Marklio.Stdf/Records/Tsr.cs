using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// TSR — Test Synopsis Record (10, 30).
/// Contains summary statistics for a single test across all parts.
/// </summary>
[StdfRecord(10, 30)]
public partial record struct Tsr : IHeadSiteRecord
{
    public byte HeadNumber { get; set; }
    public byte SiteNumber { get; set; }
    [C1] public char? TestType { get; set; }
    public uint? TestNumber { get; set; }
    public uint? ExecutedCount { get; set; }
    public uint? FailedCount { get; set; }
    public uint? AlarmCount { get; set; }
    public string? TestName { get; set; }
    public string? SequencerName { get; set; }
    public string? TestLabel { get; set; }
    [BitField] public byte? OptionalFlags { get; set; }
    public float? TestTimeAverage { get; set; }
    public float? ResultMin { get; set; }
    public float? ResultMax { get; set; }
    public float? ResultSum { get; set; }
    public float? ResultSumOfSquares { get; set; }
}
