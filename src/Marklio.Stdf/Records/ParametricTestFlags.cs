namespace Marklio.Stdf.Records;

/// <summary>
/// Parametric test flags for <see cref="Ptr"/> (PARM_FLG).
/// Indicates measurement anomalies and limit comparisons.
/// </summary>
[Flags]
public enum ParametricTestFlags : byte
{
    /// <summary>No flags set.</summary>
    None = 0,

    /// <summary>Scale error detected.</summary>
    ScaleError = 0x01,

    /// <summary>Drift error detected.</summary>
    DriftError = 0x02,

    /// <summary>Oscillation detected.</summary>
    Oscillation = 0x04,

    /// <summary>Result is above the high limit.</summary>
    AboveHighLimit = 0x08,

    /// <summary>Result is below the low limit.</summary>
    BelowLowLimit = 0x10,

    /// <summary>Low limit comparison passed.</summary>
    LowLimitPassed = 0x20,

    /// <summary>High limit comparison passed.</summary>
    HighLimitPassed = 0x40,

    /// <summary>Result equals the low limit and passed.</summary>
    EqualLowLimitPassed = 0x80,
}