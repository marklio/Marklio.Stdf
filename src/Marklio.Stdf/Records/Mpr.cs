using System.Collections;
using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// MPR — Multiple-Result Parametric Record (15, 15).
/// Contains results from a parametric test with multiple measurements. One per test per part.
/// </summary>
[StdfRecord(15, 15)]
public partial record struct Mpr : ITestRecord
{
    /// <summary>Test number from the test program. [STDF: TEST_NUM, U*4]</summary>
    public uint TestNumber { get; set; }

    /// <summary>Test head number. [STDF: HEAD_NUM, U*1]</summary>
    public byte HeadNumber { get; set; }

    /// <summary>Test site number. [STDF: SITE_NUM, U*1]</summary>
    public byte SiteNumber { get; set; }

    /// <summary>Test result flags (same encoding as PTR). [STDF: TEST_FLG, B*1]</summary>
    [BitField] public byte TestFlags { get; set; }

    /// <summary>Parametric test flags (same encoding as PTR). [STDF: PARM_FLG, B*1]</summary>
    [BitField] public byte ParametricFlags { get; set; }

    [WireCount("rtn")] private ushort ReturnResultCount => throw new NotSupportedException();
    [WireCount("rst")] private ushort ReturnStateCount => throw new NotSupportedException();

    /// <summary>Array of returned test results. [STDF: RTN_RSLT, kxR*4]</summary>
    /// <remarks>Counted by RTN_ICNT (shared-count group "rtn").</remarks>
    [CountedArray("rtn")] public float[]? ReturnResults { get; set; }

    /// <summary>Array of return states (pass/fail per pin). [STDF: RTN_STAT, kxN*1]</summary>
    /// <remarks>
    /// Counted by RSLT_CNT (shared-count group "rst").
    /// Nibble-packed on the wire: two states per byte.
    /// </remarks>
    [CountedArray("rst"), Nibble] public byte[]? ReturnStates { get; set; }

    /// <summary>Descriptive test text. [STDF: TEST_TXT, C*n]</summary>
    public string? TestText { get; set; }

    /// <summary>Alarm name or ID. [STDF: ALARM_ID, C*n]</summary>
    public string? AlarmId { get; set; }

    /// <summary>Optional data flags (same encoding as PTR). [STDF: OPT_FLAG, B*1]</summary>
    [BitField] public byte? OptionalFlags { get; set; }

    /// <summary>Exponent for scaling results. [STDF: RES_SCAL, I*1]</summary>
    public sbyte? ResultExponent { get; set; }

    /// <summary>Exponent for scaling low limit. [STDF: LLM_SCAL, I*1]</summary>
    public sbyte? LowLimitExponent { get; set; }

    /// <summary>Exponent for scaling high limit. [STDF: HLM_SCAL, I*1]</summary>
    public sbyte? HighLimitExponent { get; set; }

    /// <summary>Low test limit. [STDF: LO_LIMIT, R*4]</summary>
    public float? LowLimit { get; set; }

    /// <summary>High test limit. [STDF: HI_LIMIT, R*4]</summary>
    public float? HighLimit { get; set; }

    /// <summary>Starting input condition for the test. [STDF: START_IN, R*4]</summary>
    public float? StartingCondition { get; set; }

    /// <summary>Increment of the input condition between measurements. [STDF: INCR_IN, R*4]</summary>
    public float? ConditionIncrement { get; set; }

    [WireCount("rtnIdx")] private ushort ReturnIndexCount => throw new NotSupportedException();

    /// <summary>Pin indexes (from PMR) for each returned result. [STDF: RTN_INDX, kxU*2]</summary>
    /// <remarks>Counted by RTN_INDX count (shared-count group "rtnIdx").</remarks>
    [CountedArray("rtnIdx")] public ushort[]? ReturnPinIndexes { get; set; }

    /// <summary>Test units. [STDF: UNITS, C*n]</summary>
    public string? Units { get; set; }

    /// <summary>Input condition units. [STDF: UNITS_IN, C*n]</summary>
    public string? UnitsInput { get; set; }

    /// <summary>Result format string. [STDF: C_RESFMT, C*n]</summary>
    public string? ResultFormatString { get; set; }

    /// <summary>Low limit format string. [STDF: C_LLMFMT, C*n]</summary>
    public string? LowLimitFormatString { get; set; }

    /// <summary>High limit format string. [STDF: C_HLMFMT, C*n]</summary>
    public string? HighLimitFormatString { get; set; }

    /// <summary>Low specification limit. [STDF: LO_SPEC, R*4]</summary>
    public float? LowSpecLimit { get; set; }

    /// <summary>High specification limit. [STDF: HI_SPEC, R*4]</summary>
    public float? HighSpecLimit { get; set; }
}
