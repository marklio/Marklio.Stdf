using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// SBR — Software Bin Record (1, 50).
/// Contains the count of parts sorted into a software bin. One record per bin per head/site combination.
/// </summary>
[StdfRecord(1, 50)]
public partial record struct Sbr : IBinRecord
{
    /// <summary>
    /// Test head number. A value of 255 indicates a summary for all test heads.
    /// [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; set; }

    /// <summary>
    /// Test site number. A value of 255 indicates a summary for all test sites.
    /// [STDF: SITE_NUM, U*1]
    /// </summary>
    public byte SiteNumber { get; set; }

    /// <summary>
    /// Software bin number.
    /// [STDF: SBIN_NUM, U*2]
    /// </summary>
    public ushort SoftwareBin { get; set; }

    /// <summary>
    /// Number of parts in this software bin.
    /// [STDF: SBIN_CNT, U*4]
    /// </summary>
    public uint BinCount { get; set; }

    /// <summary>
    /// Pass/fail indication for this software bin ('P' = pass, 'F' = fail, ' ' = unknown). Optional.
    /// [STDF: SBIN_PF, C*1]
    /// </summary>
    /// <remarks>
    /// Serialized as a single ASCII byte (C*1) and converted to a <see cref="char"/>.
    /// </remarks>
    [C1] public char? BinPassFail { get; set; }

    /// <summary>
    /// Name or description of this software bin. Optional.
    /// [STDF: SBIN_NAM, C*n]
    /// </summary>
    public string? BinName { get; set; }

    // IBinRecord explicit implementations — map generic names to SBR-specific properties
    ushort IBinRecord.BinNumber => SoftwareBin;
    char? IBinRecord.PassFail => BinPassFail;
}
