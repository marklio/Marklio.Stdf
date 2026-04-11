using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PIR — Part Information Record (5, 10).
/// Marks the beginning of a new part test sequence.
/// </summary>
[StdfRecord(5, 10)]
public partial record struct Pir : IHeadSiteRecord
{
    /// <summary>Test head number.</summary>
    public byte HeadNumber { get; set; }

    /// <summary>Test site number.</summary>
    public byte SiteNumber { get; set; }
}
