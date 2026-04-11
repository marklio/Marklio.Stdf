using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// NMR — Name Map Record (1, 91).
/// V4-2007. Maps logical names (scan chain names, signal names) to PMR indices.
/// Arrays are counted by LOCM_CNT (local count for this record).
/// TOTM_CNT provides the total count across all continuation records.
/// </summary>
[StdfRecord(1, 91)]
public partial record struct Nmr
{
    [BitField] public byte ContinuationFlag { get; set; }
    public ushort TotalMapCount { get; set; }

    [WireCount("loc")] private ushort LocalMapCount => throw new NotSupportedException();

    [CountedArray("loc")] public ushort[]? PmrIndexes { get; set; }
    [CountedArray("loc")] public string[]? AtpgNames { get; set; }
}
