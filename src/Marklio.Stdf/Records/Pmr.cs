using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PMR — Pin Map Record (1, 60).
/// Maps a pin index to a physical/logical pin. One record per pin.
/// </summary>
[StdfRecord(1, 60)]
public partial record class Pmr
{
    /// <summary>
    /// Unique index for this pin, referenced by test result records.
    /// [STDF: PMR_INDX, U*2]
    /// </summary>
    public ushort PinIndex { get; set; }

    /// <summary>
    /// Channel type.
    /// [STDF: CHAN_TYP, U*2]
    /// </summary>
    public ushort? ChannelType { get; set; }

    /// <summary>
    /// Channel name.
    /// [STDF: CHAN_NAM, C*n]
    /// </summary>
    public string? ChannelName { get; set; }

    /// <summary>
    /// Physical pin name.
    /// [STDF: PHY_NAM, C*n]
    /// </summary>
    public string? PhysicalName { get; set; }

    /// <summary>
    /// Logical pin name.
    /// [STDF: LOG_NAM, C*n]
    /// </summary>
    public string? LogicalName { get; set; }

    /// <summary>
    /// Test head number associated with this pin.
    /// [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte? HeadNumber { get; set; }

    /// <summary>
    /// Test site number associated with this pin.
    /// [STDF: SITE_NUM, U*1]
    /// </summary>
    public byte? SiteNumber { get; set; }
}
