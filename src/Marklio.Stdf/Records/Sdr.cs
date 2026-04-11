using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// SDR — Site Description Record (1, 80).
/// Contains configuration information for a test site.
/// </summary>
[StdfRecord(1, 80)]
public partial record struct Sdr : IHeadRecord
{
    public byte HeadNumber { get; set; }
    public byte SiteGroup { get; set; }

    [WireCount("sites")] private byte SiteCount => throw new NotSupportedException();
    [CountedArray("sites")] public byte[]? SiteNumbers { get; set; }

    public string? HandlerType { get; set; }
    public string? HandlerId { get; set; }
    public string? CardType { get; set; }
    public string? CardId { get; set; }
    public string? LoadboardType { get; set; }
    public string? LoadboardId { get; set; }
    public string? DibType { get; set; }
    public string? DibId { get; set; }
    public string? CableType { get; set; }
    public string? CableId { get; set; }
    public string? ContactorType { get; set; }
    public string? ContactorId { get; set; }
    public string? LaserType { get; set; }
    public string? LaserId { get; set; }
    public string? ExtraType { get; set; }
    public string? ExtraId { get; set; }
}
