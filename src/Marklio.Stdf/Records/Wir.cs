using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// WIR — Wafer Information Record (2, 10).
/// Marks the start of testing on a wafer. One per wafer.
/// </summary>
[StdfRecord(2, 10)]
public partial record class Wir : IHeadRecord
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
    /// <remarks>
    /// Wire format is a 32-bit unsigned Unix epoch timestamp, mapped to <see cref="DateTime"/> via the [StdfDateTime] attribute.
    /// Valid range: 1970-01-01T00:00:01Z to 2106-02-07T06:28:15Z. <c>default(DateTime)</c> maps to 0 ("not specified").
    /// </remarks>
    [StdfDateTime] public DateTime? StartTime { get; set; }

    /// <summary>
    /// Wafer identification string. [STDF: WAFER_ID, C*n]
    /// </summary>
    public string? WaferId { get; set; }
}
