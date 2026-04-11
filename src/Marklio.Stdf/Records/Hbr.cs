using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// HBR — Hardware Bin Record (1, 40).
/// Contains hardware bin information for a test head/site.
/// </summary>
[StdfRecord(1, 40)]
public partial record struct Hbr : IBinRecord
{
    public byte HeadNumber { get; set; }
    public byte SiteNumber { get; set; }
    public ushort HardwareBin { get; set; }
    public uint BinCount { get; set; }
    [C1] public char? BinPassFail { get; set; }
    public string? BinName { get; set; }

    // IBinRecord explicit implementations — map generic names to HBR-specific properties
    ushort IBinRecord.BinNumber => HardwareBin;
    char? IBinRecord.PassFail => BinPassFail;
}
