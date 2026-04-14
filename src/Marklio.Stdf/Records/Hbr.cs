using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// HBR — Hardware Bin Record (1, 40).
/// Contains the count of parts sorted into a hardware bin. One record per bin per head/site combination.
/// </summary>
[StdfRecord(1, 40)]
public partial record class Hbr : IBinRecord
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
    /// Hardware bin number.
    /// [STDF: HBIN_NUM, U*2]
    /// </summary>
    public ushort HardwareBin { get; set; }

    /// <summary>
    /// Number of parts in this hardware bin.
    /// [STDF: HBIN_CNT, U*4]
    /// </summary>
    public uint BinCount { get; set; }

    /// <summary>
    /// Pass/fail indication for this hardware bin ('P' = pass, 'F' = fail, ' ' = unknown). Optional.
    /// [STDF: HBIN_PF, C*1]
    /// </summary>
    /// <remarks>
    /// Serialized as a single ASCII byte (C*1) and converted to a <see cref="char"/>.
    /// </remarks>
    [C1] public char? BinPassFail { get; set; }

    /// <summary>
    /// Name or description of this hardware bin. Optional.
    /// [STDF: HBIN_NAM, C*n]
    /// </summary>
    public string? BinName { get; set; }
}
