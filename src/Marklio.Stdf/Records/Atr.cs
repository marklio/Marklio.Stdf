using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// Audit Trail Record (ATR) — type 0, subtype 20.
/// Records modifications or conversions that have been applied to the STDF file
/// after its original creation.
/// </summary>
[StdfRecord(0, 20)]
public partial record struct Atr
{
    /// <summary>
    /// Date and time of the file modification. [STDF: MOD_TIM, U*4]
    /// </summary>
    /// <remarks>Wire format is a 32-bit unsigned Unix epoch timestamp, mapped to <see cref="DateTime"/> via the [StdfDateTime] attribute.</remarks>
    [StdfDateTime] public DateTime ModifiedTime { get; set; }

    /// <summary>
    /// Command line or description of the modification performed. [STDF: CMD_LINE, C*n]
    /// </summary>
    public string? CommandLine { get; set; }
}
