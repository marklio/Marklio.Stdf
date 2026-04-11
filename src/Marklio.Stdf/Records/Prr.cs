using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PRR — Part Results Record (5, 20).
/// Contains the result information for a tested part.
/// </summary>
[StdfRecord(5, 20)]
public partial record struct Prr : IHeadSiteRecord
{
    public byte HeadNumber { get; set; }
    public byte SiteNumber { get; set; }
    [BitField] public byte PartFlag { get; set; }
    public ushort NumTestsExecuted { get; set; }
    public ushort HardwareBin { get; set; }
    public ushort? SoftwareBin { get; set; }
    public short? XCoordinate { get; set; }
    public short? YCoordinate { get; set; }
    public uint? TestTime { get; set; }
    public string? PartId { get; set; }
    public string? PartText { get; set; }
    [BitEncoded] public byte[]? PartFix { get; set; }
}
