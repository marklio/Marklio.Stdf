using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PIR — Part Information Record (5, 10).
/// Marks the beginning of a part (device) test. One per part tested.
/// </summary>
[StdfRecord(5, 10)]
public partial record class Pir : IHeadSiteRecord
{
    /// <summary>
    /// Test head number. [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; set; }

    /// <summary>
    /// Test site number. [STDF: SITE_NUM, U*1]
    /// </summary>
    public byte SiteNumber { get; set; }
}
