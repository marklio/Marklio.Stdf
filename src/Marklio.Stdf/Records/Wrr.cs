using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// WRR — Wafer Results Record (2, 20).
/// Contains wafer-level test results.
/// </summary>
[StdfRecord(2, 20)]
public partial record struct Wrr : IHeadRecord
{
    public byte HeadNumber { get; set; }
    public byte SiteGroup { get; set; }
    [StdfDateTime] public DateTime FinishTime { get; set; }
    public uint PartCount { get; set; }
    public uint? RetestCount { get; set; }
    public uint? AbortCount { get; set; }
    public uint? GoodCount { get; set; }
    public uint? FunctionalCount { get; set; }
    public string? WaferId { get; set; }
    public string? FabWaferId { get; set; }
    public string? FrameId { get; set; }
    public string? MaskId { get; set; }
    public string? UserDescription { get; set; }
    public string? ExecDescription { get; set; }
}
