using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// WRR — Wafer Results Record (2, 20).
/// Contains summary results for a wafer. One per wafer.
/// </summary>
[StdfRecord(2, 20)]
public partial record struct Wrr : IHeadRecord
{
    /// <summary>
    /// Test head number. [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; set; }

    /// <summary>
    /// Site group number. [STDF: SITE_GRP, U*1]
    /// </summary>
    public byte SiteGroup { get; set; }

    /// <summary>
    /// Date and time wafer testing completed. [STDF: FINISH_T, U*4]
    /// </summary>
    /// <remarks>Wire format is a 32-bit unsigned Unix epoch timestamp, mapped to <see cref="DateTime"/> via the [StdfDateTime] attribute.</remarks>
    [StdfDateTime] public DateTime FinishTime { get; set; }

    /// <summary>
    /// Number of parts tested on this wafer. [STDF: PART_CNT, U*4]
    /// </summary>
    public uint PartCount { get; set; }

    /// <summary>
    /// Number of parts retested. [STDF: RTST_CNT, U*4]
    /// </summary>
    public uint? RetestCount { get; set; }

    /// <summary>
    /// Number of test aborts. [STDF: ABRT_CNT, U*4]
    /// </summary>
    public uint? AbortCount { get; set; }

    /// <summary>
    /// Number of good (passing) parts. [STDF: GOOD_CNT, U*4]
    /// </summary>
    public uint? GoodCount { get; set; }

    /// <summary>
    /// Number of functional parts. [STDF: FUNC_CNT, U*4]
    /// </summary>
    public uint? FunctionalCount { get; set; }

    /// <summary>
    /// Wafer identification string. [STDF: WAFER_ID, C*n]
    /// </summary>
    public string? WaferId { get; set; }

    /// <summary>
    /// Fabrication wafer ID. [STDF: FABWF_ID, C*n]
    /// </summary>
    public string? FabWaferId { get; set; }

    /// <summary>
    /// Wafer frame ID. [STDF: FRAME_ID, C*n]
    /// </summary>
    public string? FrameId { get; set; }

    /// <summary>
    /// Wafer mask ID. [STDF: MASK_ID, C*n]
    /// </summary>
    public string? MaskId { get; set; }

    /// <summary>
    /// User-supplied wafer description. [STDF: USR_DESC, C*n]
    /// </summary>
    public string? UserDescription { get; set; }

    /// <summary>
    /// Executive-supplied wafer description. [STDF: EXC_DESC, C*n]
    /// </summary>
    public string? ExecDescription { get; set; }
}
