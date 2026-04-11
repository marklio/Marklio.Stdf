using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PSR — Pattern Sequence Record (1, 90).
/// V4-2007. Defines scan pattern sequences, including begin/end pattern indices and file names.
/// Arrays are counted by LOCP_CNT (local count for this record).
/// TOTP_CNT provides the total count across all continuation records.
/// </summary>
[StdfRecord(1, 90)]
public partial record struct Psr
{
    [BitField] public byte ContinuationFlag { get; set; }
    public ushort PsrIndex { get; set; }
    public string? PsrName { get; set; }
    [BitField] public byte? OptionalFlags { get; set; }
    public ushort? TotalPatternCount { get; set; }

    [WireCount("loc")] private ushort LocalPatternCount => throw new NotSupportedException();

    [CountedArray("loc")] public ulong[]? PatternBegin { get; set; }
    [CountedArray("loc")] public ulong[]? PatternEnd { get; set; }
    [CountedArray("loc")] public string[]? PatternFiles { get; set; }
    [CountedArray("loc")] public string[]? PatternLabels { get; set; }
    [CountedArray("loc")] public string[]? FileUids { get; set; }
    [CountedArray("loc")] public string[]? AtpgDescriptions { get; set; }
    [CountedArray("loc")] public string[]? SourceIds { get; set; }
}
