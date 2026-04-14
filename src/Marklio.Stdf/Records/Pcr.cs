using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PCR — Part Count Record (1, 30).
/// Contains part count totals per head/site at end of testing.
/// </summary>
[StdfRecord(1, 30)]
public partial record class Pcr : IHeadSiteRecord
{
    /// <summary>
    /// Test head number (255 = summary for all heads). [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; set; }

    /// <summary>
    /// Test site number (255 = summary for all sites). [STDF: SITE_NUM, U*1]
    /// </summary>
    public byte SiteNumber { get; set; }

    /// <summary>
    /// Number of parts tested. [STDF: PART_CNT, U*4]
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
}
