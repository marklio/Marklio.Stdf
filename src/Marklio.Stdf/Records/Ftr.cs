using System.Collections;
using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// FTR — Functional Test Record (15, 20).
/// Contains results of a single functional test execution. One per test per part.
/// </summary>
[StdfRecord(15, 20)]
public partial record struct Ftr : ITestRecord
{
    /// <summary>Test number from the test program. [STDF: TEST_NUM, U*4]</summary>
    public uint TestNumber { get; set; }

    /// <summary>Test head number. [STDF: HEAD_NUM, U*1]</summary>
    public byte HeadNumber { get; set; }

    /// <summary>Test site number. [STDF: SITE_NUM, U*1]</summary>
    public byte SiteNumber { get; set; }

    /// <summary>Test result flags (same encoding as PTR). [STDF: TEST_FLG, B*1]</summary>
    [BitField] public byte? TestFlags { get; set; }

    /// <summary>
    /// Optional flags. Bit 0=CYC_CNT invalid, bit 1=REL_VADR invalid,
    /// bit 2=RPT_CNT invalid, bit 3=NUM_FAIL invalid,
    /// bit 4=XFAIL_AD/YFAIL_AD invalid, bit 5=VECT_OFF invalid.
    /// [STDF: OPT_FLAG, B*1]
    /// </summary>
    [BitField] public byte? OptionalFlags { get; set; }

    /// <summary>Cycle count of the failing vector. [STDF: CYC_CNT, U*4]</summary>
    public uint? CycleCount { get; set; }

    /// <summary>Relative vector address of the failure. [STDF: REL_VADR, U*4]</summary>
    public uint? RelativeVectorAddress { get; set; }

    /// <summary>Repeat count of the failing vector. [STDF: RPT_CNT, U*4]</summary>
    public uint? RepeatCount { get; set; }

    /// <summary>Number of pins failing. [STDF: NUM_FAIL, U*4]</summary>
    public uint? FailCount { get; set; }

    /// <summary>X logical device failure address. [STDF: XFAIL_AD, I*4]</summary>
    public int? XFailAddress { get; set; }

    /// <summary>Y logical device failure address. [STDF: YFAIL_AD, I*4]</summary>
    public int? YFailAddress { get; set; }

    /// <summary>Offset from the pattern burst to the programmed vector. [STDF: VECT_OFF, I*2]</summary>
    public short? VectorOffset { get; set; }

    [WireCount("rtn")] private ushort ReturnCount => throw new NotSupportedException();
    [WireCount("pgm")] private ushort ProgramCount => throw new NotSupportedException();

    /// <summary>Return pin indexes (from PMR). [STDF: RTN_INDX, jxU*2]</summary>
    /// <remarks>Shared-count group "rtn" with <see cref="ReturnStates"/>; both arrays are sized by RTN_ICNT.</remarks>
    [CountedArray("rtn")] public ushort[]? ReturnIndexes { get; set; }

    /// <summary>Return states per pin. [STDF: RTN_STAT, jxN*1]</summary>
    /// <remarks>
    /// Shared-count group "rtn" with <see cref="ReturnIndexes"/>; both arrays are sized by RTN_ICNT.
    /// Nibble-packed on the wire: two states per byte.
    /// </remarks>
    [CountedArray("rtn"), Nibble] public byte[]? ReturnStates { get; set; }

    /// <summary>Programmed state pin indexes (from PMR). [STDF: PGM_INDX, kxU*2]</summary>
    /// <remarks>Shared-count group "pgm" with <see cref="ProgramStates"/>; both arrays are sized by PGM_ICNT.</remarks>
    [CountedArray("pgm")] public ushort[]? ProgramIndexes { get; set; }

    /// <summary>Programmed states per pin. [STDF: PGM_STAT, kxN*1]</summary>
    /// <remarks>
    /// Shared-count group "pgm" with <see cref="ProgramIndexes"/>; both arrays are sized by PGM_ICNT.
    /// Nibble-packed on the wire: two states per byte.
    /// </remarks>
    [CountedArray("pgm"), Nibble] public byte[]? ProgramStates { get; set; }

    /// <summary>Failing pin bitfield. [STDF: FAIL_PIN, D*n]</summary>
    /// <remarks>Wire format is bit-count-prefixed data deserialized into a <see cref="BitArray"/>.</remarks>
    [BitArray] public BitArray? FailingPins { get; set; }

    /// <summary>Vector module pattern name. [STDF: VECT_NAM, C*n]</summary>
    public string? VectorName { get; set; }

    /// <summary>Time set name. [STDF: TIME_SET, C*n]</summary>
    public string? TimeSet { get; set; }

    /// <summary>Vector op code. [STDF: OP_CODE, C*n]</summary>
    public string? OpCode { get; set; }

    /// <summary>Descriptive test text. [STDF: TEST_TXT, C*n]</summary>
    public string? TestText { get; set; }

    /// <summary>Alarm name or ID. [STDF: ALARM_ID, C*n]</summary>
    public string? AlarmId { get; set; }

    /// <summary>Program text (additional info). [STDF: PROG_TXT, C*n]</summary>
    public string? ProgramText { get; set; }

    /// <summary>Result text (additional info). [STDF: RSLT_TXT, C*n]</summary>
    public string? ResultText { get; set; }

    /// <summary>Pattern generator number. [STDF: PATG_NUM, U*1]</summary>
    public byte? PatternGeneratorNumber { get; set; }

    /// <summary>Spin map (enabled comparators). [STDF: SPIN_MAP, D*n]</summary>
    /// <remarks>Wire format is bit-count-prefixed data deserialized into a <see cref="BitArray"/>.</remarks>
    [BitArray] public BitArray? SpinMap { get; set; }

    /// <summary>Gets <see cref="TestFlags"/> as a typed enum. See <see cref="TestFlags"/> for the raw value.</summary>
    public TestResultFlags TestFlagsEnum => (TestResultFlags)(TestFlags ?? 0);

    /// <summary>Gets <see cref="OptionalFlags"/> as a typed enum. See <see cref="OptionalFlags"/> for the raw value.</summary>
    public FtrOptionalFlags OptionalFlagsEnum => (FtrOptionalFlags)(OptionalFlags ?? 0);
}
