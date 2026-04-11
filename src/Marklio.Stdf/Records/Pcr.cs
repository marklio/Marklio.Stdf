using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PCR — Part Count Record (1, 30).
/// Contains test part count information for a test head/site.
/// </summary>
[StdfRecord(1, 30)]
public partial record struct Pcr : IHeadSiteRecord
{
    public byte HeadNumber { get; set; }
    public byte SiteNumber { get; set; }
    public uint PartCount { get; set; }
    public uint? RetestCount { get; set; }
    public uint? AbortCount { get; set; }
    public uint? GoodCount { get; set; }
    public uint? FunctionalCount { get; set; }
}
