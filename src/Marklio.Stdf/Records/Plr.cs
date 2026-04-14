using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PLR — Pin List Record (1, 63).
/// Contains pin group configuration and mode information. One record per pin list definition.
/// </summary>
[StdfRecord(1, 63)]
public partial record class Plr
{
    [WireCount("grp")] private ushort GroupCount => throw new NotSupportedException();

    /// <summary>
    /// Pin group indexes.
    /// [STDF: GRP_INDX, kxU*2]
    /// </summary>
    /// <remarks>
    /// Shared-count array (group "grp"). All arrays in this group share a single wire count
    /// derived from the first array's length during serialization. All arrays should have the same length.
    /// </remarks>
    [CountedArray("grp")] public ushort[]? GroupIndexes { get; set; }

    /// <summary>
    /// Operating mode for each pin group (0=unknown, 10=normal, 20=BIST, etc.).
    /// [STDF: GRP_MODE, kxU*2]
    /// </summary>
    /// <remarks>
    /// Shared-count array (group "grp"). All arrays in this group share a single wire count
    /// derived from the first array's length during serialization. All arrays should have the same length.
    /// </remarks>
    [CountedArray("grp")] public ushort[]? GroupModes { get; set; }

    /// <summary>
    /// Display radix for each group (0=default, 2=binary, 8=octal, 10=decimal, 16=hex, 20=symbolic).
    /// [STDF: GRP_RADX, kxU*1]
    /// </summary>
    /// <remarks>
    /// Shared-count array (group "grp"). All arrays in this group share a single wire count
    /// derived from the first array's length during serialization. All arrays should have the same length.
    /// </remarks>
    [CountedArray("grp")] public byte[]? GroupRadixes { get; set; }

    /// <summary>
    /// Program-state encoding characters for each group.
    /// [STDF: PGM_CHAR, kxC*n]
    /// </summary>
    /// <remarks>
    /// Shared-count array (group "grp"). All arrays in this group share a single wire count
    /// derived from the first array's length during serialization. All arrays should have the same length.
    /// </remarks>
    [CountedArray("grp")] public string[]? ProgramChars { get; set; }

    /// <summary>
    /// Return-state encoding characters for each group.
    /// [STDF: RTN_CHAR, kxC*n]
    /// </summary>
    /// <remarks>
    /// Shared-count array (group "grp"). All arrays in this group share a single wire count
    /// derived from the first array's length during serialization. All arrays should have the same length.
    /// </remarks>
    [CountedArray("grp")] public string[]? ReturnChars { get; set; }

    /// <summary>
    /// Long program-state encoding characters for each group.
    /// [STDF: PGM_CHAL, kxC*n]
    /// </summary>
    /// <remarks>
    /// Shared-count array (group "grp"). All arrays in this group share a single wire count
    /// derived from the first array's length during serialization. All arrays should have the same length.
    /// </remarks>
    [CountedArray("grp")] public string[]? ProgramCharsLong { get; set; }

    /// <summary>
    /// Long return-state encoding characters for each group.
    /// [STDF: RTN_CHAL, kxC*n]
    /// </summary>
    /// <remarks>
    /// Shared-count array (group "grp"). All arrays in this group share a single wire count
    /// derived from the first array's length during serialization. All arrays should have the same length.
    /// </remarks>
    [CountedArray("grp")] public string[]? ReturnCharsLong { get; set; }
}
