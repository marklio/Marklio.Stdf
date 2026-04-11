using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// WIR — Wafer Information Record (2, 10).
/// Marks the beginning of wafer testing.
/// </summary>
[StdfRecord(2, 10)]
public partial record struct Wir : IHeadRecord
{
    public byte HeadNumber { get; set; }
    public byte? SiteGroup { get; set; }
    [StdfDateTime] public DateTime? StartTime { get; set; }
    public string? WaferId { get; set; }
}
