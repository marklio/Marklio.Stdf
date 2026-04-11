namespace Marklio.Stdf.Records;

/// <summary>
/// Part result flags for <see cref="Prr"/> (PART_FLG).
/// Indicates part disposition and testing conditions.
/// </summary>
[Flags]
public enum PartResultFlags : byte
{
    /// <summary>No flags set.</summary>
    None = 0,

    /// <summary>This part supersedes a previously tested part (new part ID).</summary>
    SupersedesPrevious = 0x01,

    /// <summary>Part was tested at multiple test heads.</summary>
    MultipleHeads = 0x02,

    /// <summary>Abnormal end of testing.</summary>
    AbnormalEnd = 0x04,

    /// <summary>Part failed.</summary>
    Failed = 0x08,

    /// <summary>No pass/fail indication (informational test only).</summary>
    NoPassFailIndication = 0x10,
}
