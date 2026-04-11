using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PGR — Pin Group Record (1, 62).
/// Associates a group of pins under a group index and name.
/// </summary>
[StdfRecord(1, 62)]
public partial record struct Pgr
{
    public ushort GroupIndex { get; set; }
    public string? GroupName { get; set; }

    [WireCount("grp")] private ushort PinCount => throw new NotSupportedException();
    [CountedArray("grp")] public ushort[]? PinIndexes { get; set; }
}
