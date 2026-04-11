using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// WIR — Wafer Information Record (2, 10).
/// Marks the start of testing on a wafer. One per wafer.
/// </summary>
[StdfRecord(2, 10)]
public partial record struct Wir : IHeadRecord
{
    /// <summary>
    /// Test head number. [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; set; }

    /// <summary>
    /// Site group number. [STDF: SITE_GRP, U*1]
    /// </summary>
    public byte? SiteGroup { get; set; }

    /// <summary>
    /// Date and time wafer testing started. [STDF: START_T, U*4]
    /// </summary>
    /// <remarks>Wire format is a 32-bit unsigned Unix epoch timestamp, mapped to <see cref="DateTime"/> via the [StdfDateTime] attribute.</remarks>
    [StdfDateTime] public DateTime? StartTime { get; set; }

    /// <summary>
    /// Wafer identification string. [STDF: WAFER_ID, C*n]
    /// </summary>
    public string? WaferId { get; set; }
}
