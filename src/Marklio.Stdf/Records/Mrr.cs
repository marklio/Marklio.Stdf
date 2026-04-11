using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// Master Results Record (MRR) — type 1, subtype 20.
/// Marks the end of the STDF file and contains completion information
/// for the test lot, including finish time and disposition.
/// </summary>
[StdfRecord(1, 20)]
public partial record struct Mrr
{
    /// <summary>
    /// Date and time testing completed. [STDF: FINISH_T, U*4]
    /// </summary>
    /// <remarks>Wire format is a 32-bit unsigned Unix epoch timestamp, mapped to <see cref="DateTime"/> via the [StdfDateTime] attribute.</remarks>
    [StdfDateTime] public DateTime FinishTime { get; set; }

    /// <summary>
    /// Lot disposition code. [STDF: DISP_COD, C*1]
    /// </summary>
    /// <remarks>Wire format is a single ASCII byte, mapped to <see cref="char"/> via the [C1] attribute.</remarks>
    [C1] public char? DispositionCode { get; set; }

    /// <summary>
    /// User-supplied lot description. [STDF: USR_DESC, C*n]
    /// </summary>
    public string? UserDescription { get; set; }

    /// <summary>
    /// Executive-supplied lot description. [STDF: EXC_DESC, C*n]
    /// </summary>
    public string? ExecDescription { get; set; }
}
