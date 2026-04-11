using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// WCR — Wafer Configuration Record (2, 30).
/// Contains wafer-level geometry and orientation information.
/// </summary>
[StdfRecord(2, 30)]
public partial record struct Wcr
{
    /// <summary>
    /// Diameter of the wafer in the units specified by <see cref="WaferUnits"/>. [STDF: WAFR_SIZ, R*4]
    /// </summary>
    public float? WaferSize { get; set; }

    /// <summary>
    /// Height of die in wafer units. [STDF: DIE_HT, R*4]
    /// </summary>
    public float? DieHeight { get; set; }

    /// <summary>
    /// Width of die in wafer units. [STDF: DIE_WID, R*4]
    /// </summary>
    public float? DieWidth { get; set; }

    /// <summary>
    /// Units for wafer and die dimensions (0=unknown, 1=inches, 2=centimeters, 3=millimeters, 4=mils). [STDF: WF_UNITS, U*1]
    /// </summary>
    public byte? WaferUnits { get; set; }

    /// <summary>
    /// Orientation of wafer flat (U=up, D=down, L=left, R=right). [STDF: WF_FLAT, C*1]
    /// </summary>
    /// <remarks>Wire format is a single ASCII byte, mapped to <see cref="char"/> via the [C1] attribute.</remarks>
    [C1] public char? WaferFlat { get; set; }

    /// <summary>
    /// X coordinate of center die. [STDF: CENTER_X, I*2]
    /// </summary>
    public short? CenterX { get; set; }

    /// <summary>
    /// Y coordinate of center die. [STDF: CENTER_Y, I*2]
    /// </summary>
    public short? CenterY { get; set; }

    /// <summary>
    /// Direction of positive X (L=left, R=right). [STDF: POS_X, C*1]
    /// </summary>
    /// <remarks>Wire format is a single ASCII byte, mapped to <see cref="char"/> via the [C1] attribute.</remarks>
    [C1] public char? PositiveX { get; set; }

    /// <summary>
    /// Direction of positive Y (U=up, D=down). [STDF: POS_Y, C*1]
    /// </summary>
    /// <remarks>Wire format is a single ASCII byte, mapped to <see cref="char"/> via the [C1] attribute.</remarks>
    [C1] public char? PositiveY { get; set; }
}
