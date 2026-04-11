using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PGR — Pin Group Record (1, 62).
/// Defines a named group of pins. One record per group.
/// </summary>
[StdfRecord(1, 62)]
public partial record struct Pgr
{
    /// <summary>
    /// Unique index for this pin group.
    /// [STDF: GRP_INDX, U*2]
    /// </summary>
    public ushort GroupIndex { get; set; }

    /// <summary>
    /// Name of the pin group.
    /// [STDF: GRP_NAM, C*n]
    /// </summary>
    public string? GroupName { get; set; }

    [WireCount("grp")] private ushort PinCount => throw new NotSupportedException();

    /// <summary>
    /// Array of PMR indexes that belong to this group.
    /// [STDF: PMR_INDX, kxU*2]
    /// </summary>
    /// <remarks>
    /// Counted by GRP_CNT on the wire.
    /// </remarks>
    [CountedArray("grp")] public ushort[]? PinIndexes { get; set; }
}
