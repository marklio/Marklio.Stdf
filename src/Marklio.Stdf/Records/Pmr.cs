using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PMR — Pin Map Record (1, 60).
/// Defines a mapping of pin index to pin name and channel.
/// </summary>
[StdfRecord(1, 60)]
public partial record struct Pmr
{
    public ushort PinIndex { get; set; }
    public ushort? ChannelType { get; set; }
    public string? ChannelName { get; set; }
    public string? PhysicalName { get; set; }
    public string? LogicalName { get; set; }
    public byte? HeadNumber { get; set; }
    public byte? SiteNumber { get; set; }
}
