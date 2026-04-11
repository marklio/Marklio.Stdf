using System.Collections;
using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// MPR — Multiple-Result Parametric Record (15, 15).
/// Contains multiple parametric test results in a single record.
/// </summary>
[StdfRecord(15, 15)]
public partial record struct Mpr : ITestRecord
{
    public uint TestNumber { get; set; }
    public byte HeadNumber { get; set; }
    public byte SiteNumber { get; set; }
    [BitField] public byte TestFlags { get; set; }
    [BitField] public byte ParametricFlags { get; set; }

    [WireCount("rtn")] private ushort ReturnResultCount => throw new NotSupportedException();
    [WireCount("rst")] private ushort ReturnStateCount => throw new NotSupportedException();

    [CountedArray("rtn")] public float[]? ReturnResults { get; set; }
    [CountedArray("rst"), Nibble] public byte[]? ReturnStates { get; set; }

    public string? TestText { get; set; }
    public string? AlarmId { get; set; }
    [BitField] public byte? OptionalFlags { get; set; }
    public sbyte? ResultExponent { get; set; }
    public sbyte? LowLimitExponent { get; set; }
    public sbyte? HighLimitExponent { get; set; }
    public float? LowLimit { get; set; }
    public float? HighLimit { get; set; }
    public float? StartingCondition { get; set; }
    public float? ConditionIncrement { get; set; }

    [WireCount("rtnIdx")] private ushort ReturnIndexCount => throw new NotSupportedException();
    [CountedArray("rtnIdx")] public ushort[]? ReturnPinIndexes { get; set; }

    public string? Units { get; set; }
    public string? UnitsInput { get; set; }
    public string? ResultFormatString { get; set; }
    public string? LowLimitFormatString { get; set; }
    public string? HighLimitFormatString { get; set; }
    public float? LowSpecLimit { get; set; }
    public float? HighSpecLimit { get; set; }
}
