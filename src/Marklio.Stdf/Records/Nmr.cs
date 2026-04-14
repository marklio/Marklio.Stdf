using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// NMR — Pin Map Name Record (1, 91).
/// V4-2007. Maps PMR indexes to ATPG signal names. Supports continuation.
/// Arrays share the "loc" count group (LOCM_CNT). TOTM_CNT provides the total count across all continuations.
/// </summary>
[StdfRecord(1, 91)]
public partial record class Nmr
{
    /// <summary>
    /// Continuation flag. Bit 0: if set, this record continues in the next NMR record.
    /// [STDF: CONT_FLG, B*1]
    /// </summary>
    [BitField] public byte ContinuationFlag { get; set; }

    /// <summary>
    /// Total name map count across all continuation records.
    /// [STDF: TOTM_CNT, U*2]
    /// </summary>
    public ushort TotalMapCount { get; set; }

    [WireCount("loc")] private ushort LocalMapCount => throw new NotSupportedException();

    /// <summary>
    /// Pin map record indexes. Shared-count group "loc".
    /// [STDF: PMR_INDX, kxU*2]
    /// </summary>
    [CountedArray("loc")] public ushort[]? PmrIndexes { get; set; }

    /// <summary>
    /// ATPG signal names. Shared-count group "loc".
    /// [STDF: ATPG_NAM, kxC*n]
    /// </summary>
    [CountedArray("loc")] public string[]? AtpgNames { get; set; }
}
