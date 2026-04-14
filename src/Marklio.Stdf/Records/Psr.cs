using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PSR — Pattern Sequence Record (1, 90).
/// V4-2007. Describes pattern files and their execution ranges within a scan test flow.
/// Supports continuation — multiple PSR records can be merged to form a complete sequence.
/// Arrays share the "loc" count group (LOCP_CNT). TOTP_CNT provides the total count across all continuations.
/// </summary>
[StdfRecord(1, 90)]
public partial record class Psr
{
    /// <summary>
    /// Continuation flag. Bit 0: if set, this record continues in the next PSR record.
    /// [STDF: CONT_FLG, B*1]
    /// </summary>
    [BitField] public byte ContinuationFlag { get; set; }

    /// <summary>
    /// Unique index for this pattern sequence, referenced by STR.PSR_REF.
    /// [STDF: PSR_INDX, U*2]
    /// </summary>
    public ushort PsrIndex { get; set; }

    /// <summary>
    /// Name of the pattern sequence. Optional.
    /// [STDF: PSR_NAM, C*n]
    /// </summary>
    public string? PsrName { get; set; }

    /// <summary>
    /// Optional data flags. Optional.
    /// [STDF: OPT_FLG, B*1]
    /// </summary>
    [BitField] public byte? OptionalFlags { get; set; }

    /// <summary>
    /// Total pattern count across all continuation records. Optional.
    /// [STDF: TOTP_CNT, U*2]
    /// </summary>
    public ushort? TotalPatternCount { get; set; }

    [WireCount("loc")] private ushort LocalPatternCount => throw new NotSupportedException();

    /// <summary>
    /// Pattern begin indexes. Shared-count group "loc".
    /// [STDF: PAT_BGN, kxU*8]
    /// </summary>
    [CountedArray("loc")] public ulong[]? PatternBegin { get; set; }

    /// <summary>
    /// Pattern end indexes. Shared-count group "loc".
    /// [STDF: PAT_END, kxU*8]
    /// </summary>
    [CountedArray("loc")] public ulong[]? PatternEnd { get; set; }

    /// <summary>
    /// Pattern file names. Shared-count group "loc".
    /// [STDF: PAT_FILE, kxC*n]
    /// </summary>
    [CountedArray("loc")] public string[]? PatternFiles { get; set; }

    /// <summary>
    /// Pattern labels. Shared-count group "loc".
    /// [STDF: PAT_LBL, kxC*n]
    /// </summary>
    [CountedArray("loc")] public string[]? PatternLabels { get; set; }

    /// <summary>
    /// File unique identifiers. Shared-count group "loc".
    /// [STDF: FILE_UID, kxC*n]
    /// </summary>
    [CountedArray("loc")] public string[]? FileUids { get; set; }

    /// <summary>
    /// ATPG descriptions. Shared-count group "loc".
    /// [STDF: ATPG_DSC, kxC*n]
    /// </summary>
    [CountedArray("loc")] public string[]? AtpgDescriptions { get; set; }

    /// <summary>
    /// Source identifiers. Shared-count group "loc".
    /// [STDF: SRC_ID, kxC*n]
    /// </summary>
    [CountedArray("loc")] public string[]? SourceIds { get; set; }
}
