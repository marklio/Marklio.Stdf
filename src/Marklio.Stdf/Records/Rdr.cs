using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// RDR — Retest Data Record (1, 70).
/// Contains the list of hardware bins that were retested.
/// </summary>
[StdfRecord(1, 70)]
public partial record struct Rdr
{
    [WireCount("bins")] private ushort RetestBinCount => throw new NotSupportedException();
    [CountedArray("bins")] public ushort[]? RetestBins { get; set; }
}
