using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// SBR — Software Bin Record (1, 50).
/// Contains software bin information for a test head/site.
/// </summary>
[StdfRecord(1, 50)]
public partial record struct Sbr : IBinRecord
{
    public byte HeadNumber { get; set; }
    public byte SiteNumber { get; set; }
    public ushort SoftwareBin { get; set; }
    public uint BinCount { get; set; }
    [C1] public char? BinPassFail { get; set; }
    public string? BinName { get; set; }

    // IBinRecord explicit implementations — map generic names to SBR-specific properties
    ushort IBinRecord.BinNumber => SoftwareBin;
    char? IBinRecord.PassFail => BinPassFail;
}
