using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// RDR — Retest Data Record (1, 70).
/// Lists the hardware bins to be retested.
/// </summary>
[StdfRecord(1, 70)]
public partial record struct Rdr
{
    [WireCount("bins")] private ushort RetestBinCount => throw new NotSupportedException();

    /// <summary>
    /// Array of hardware bin numbers to be retested.
    /// [STDF: RTST_BIN, kxU*2]
    /// </summary>
    /// <remarks>
    /// Counted by NUM_BINS on the wire.
    /// </remarks>
    [CountedArray("bins")] public ushort[]? RetestBins { get; set; }
}
