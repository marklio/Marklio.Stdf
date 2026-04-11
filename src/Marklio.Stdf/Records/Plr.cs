using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PLR — Pin List Record (1, 63).
/// Contains pin group configuration information.
/// </summary>
[StdfRecord(1, 63)]
public partial record struct Plr
{
    [WireCount("grp")] private ushort GroupCount => throw new NotSupportedException();
    [CountedArray("grp")] public ushort[]? GroupIndexes { get; set; }
    [CountedArray("grp")] public ushort[]? GroupModes { get; set; }
    [CountedArray("grp")] public byte[]? GroupRadixes { get; set; }
    [CountedArray("grp")] public string[]? ProgramChars { get; set; }
    [CountedArray("grp")] public string[]? ReturnChars { get; set; }
    [CountedArray("grp")] public string[]? ProgramCharsLong { get; set; }
    [CountedArray("grp")] public string[]? ReturnCharsLong { get; set; }
}
