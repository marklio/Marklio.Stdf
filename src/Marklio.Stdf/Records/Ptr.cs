using System.Collections;
using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PTR — Parametric Test Record (15, 10).
/// Contains the results of a single parametric test execution.
/// </summary>
[StdfRecord(15, 10)]
public partial record struct Ptr : ITestRecord
{
    public uint TestNumber { get; set; }
    public byte HeadNumber { get; set; }
    public byte SiteNumber { get; set; }
    [BitField] public byte TestFlags { get; set; }
    [BitField] public byte ParametricFlags { get; set; }
    public float? Result { get; set; }
    public string? TestText { get; set; }
    public string? AlarmId { get; set; }
    [BitField] public byte? OptionalFlags { get; set; }
    public sbyte? ResultExponent { get; set; }
    public sbyte? LowLimitExponent { get; set; }
    public sbyte? HighLimitExponent { get; set; }
    public float? LowLimit { get; set; }
    public float? HighLimit { get; set; }
    public string? Units { get; set; }
    public string? ResultFormatString { get; set; }
    public string? LowLimitFormatString { get; set; }
    public string? HighLimitFormatString { get; set; }
    public float? LowSpecLimit { get; set; }
    public float? HighSpecLimit { get; set; }
}
