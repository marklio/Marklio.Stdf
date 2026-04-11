using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// WCR — Wafer Configuration Record (2, 30).
/// Contains wafer configuration and orientation information.
/// </summary>
[StdfRecord(2, 30)]
public partial record struct Wcr
{
    public float? WaferSize { get; set; }
    public float? DieHeight { get; set; }
    public float? DieWidth { get; set; }
    public byte? WaferUnits { get; set; }
    [C1] public char? WaferFlat { get; set; }
    public short? CenterX { get; set; }
    public short? CenterY { get; set; }
    [C1] public char? PositiveX { get; set; }
    [C1] public char? PositiveY { get; set; }
}
