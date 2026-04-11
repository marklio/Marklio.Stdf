using System.Collections;
using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PTR — Parametric Test Record (15, 10).
/// Contains the result of a single parametric test execution. One per test per part.
/// </summary>
[StdfRecord(15, 10)]
public partial record struct Ptr : ITestRecord
{
    /// <summary>Test number from the test program. [STDF: TEST_NUM, U*4]</summary>
    public uint TestNumber { get; set; }

    /// <summary>Test head number. [STDF: HEAD_NUM, U*1]</summary>
    public byte HeadNumber { get; set; }

    /// <summary>Test site number. [STDF: SITE_NUM, U*1]</summary>
    public byte SiteNumber { get; set; }

    /// <summary>
    /// Test result flags. Bit 0=alarm, bit 1=result invalid, bit 2=result unreliable,
    /// bit 3=timeout, bit 4=test not executed, bit 5=test aborted, bit 6=pass/fail valid,
    /// bit 7=test failed. [STDF: TEST_FLG, B*1]
    /// </summary>
    [BitField] public byte TestFlags { get; set; }

    /// <summary>
    /// Parametric test flags. Bit 0=scale error, bit 1=drift error, bit 2=oscillation,
    /// bit 3=result &gt; high limit, bit 4=result &lt; low limit, bit 5=low limit passed,
    /// bit 6=high limit passed, bit 7=equal low limit passed. [STDF: PARM_FLG, B*1]
    /// </summary>
    [BitField] public byte ParametricFlags { get; set; }

    /// <summary>Parametric test result value. [STDF: RESULT, R*4]</summary>
    public float? Result { get; set; }

    /// <summary>Descriptive test text or label. [STDF: TEST_TXT, C*n]</summary>
    public string? TestText { get; set; }

    /// <summary>Alarm name or ID. [STDF: ALARM_ID, C*n]</summary>
    public string? AlarmId { get; set; }

    /// <summary>
    /// Optional data flags indicating which limit/format fields are valid.
    /// Bit 0=RES_SCAL invalid, bit 1=no low limit, bit 2=no high limit,
    /// bit 3=no lo spec, bit 4=no hi spec, bit 5=LLM_SCAL/HLM_SCAL invalid.
    /// [STDF: OPT_FLAG, B*1]
    /// </summary>
    [BitField] public byte? OptionalFlags { get; set; }

    /// <summary>Exponent (power of 10) for scaling the test result. [STDF: RES_SCAL, I*1]</summary>
    public sbyte? ResultExponent { get; set; }

    /// <summary>Exponent (power of 10) for scaling the low limit. [STDF: LLM_SCAL, I*1]</summary>
    public sbyte? LowLimitExponent { get; set; }

    /// <summary>Exponent (power of 10) for scaling the high limit. [STDF: HLM_SCAL, I*1]</summary>
    public sbyte? HighLimitExponent { get; set; }

    /// <summary>Low test limit value. [STDF: LO_LIMIT, R*4]</summary>
    public float? LowLimit { get; set; }

    /// <summary>High test limit value. [STDF: HI_LIMIT, R*4]</summary>
    public float? HighLimit { get; set; }

    /// <summary>Test units (e.g. "V", "A", "ohm"). [STDF: UNITS, C*n]</summary>
    public string? Units { get; set; }

    /// <summary>Result format string (C-style printf format). [STDF: C_RESFMT, C*n]</summary>
    public string? ResultFormatString { get; set; }

    /// <summary>Low limit format string. [STDF: C_LLMFMT, C*n]</summary>
    public string? LowLimitFormatString { get; set; }

    /// <summary>High limit format string. [STDF: C_HLMFMT, C*n]</summary>
    public string? HighLimitFormatString { get; set; }

    /// <summary>Low specification limit value. [STDF: LO_SPEC, R*4]</summary>
    public float? LowSpecLimit { get; set; }

    /// <summary>High specification limit value. [STDF: HI_SPEC, R*4]</summary>
    public float? HighSpecLimit { get; set; }
}
