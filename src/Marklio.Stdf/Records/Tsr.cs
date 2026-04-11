using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// TSR — Test Synopsis Record (10, 30).
/// Contains summary statistics for a single test across all executions. One record per test per head/site combination.
/// </summary>
[StdfRecord(10, 30)]
public partial record struct Tsr : IHeadSiteRecord
{
    /// <summary>
    /// Test head number. A value of 255 indicates a summary for all test heads.
    /// [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; set; }

    /// <summary>
    /// Test site number. A value of 255 indicates a summary for all test sites.
    /// [STDF: SITE_NUM, U*1]
    /// </summary>
    public byte SiteNumber { get; set; }

    /// <summary>
    /// Test type ('P' = parametric, 'F' = functional, 'M' = multi-result parametric, ' ' = unknown). Optional.
    /// [STDF: TEST_TYP, C*1]
    /// </summary>
    /// <remarks>
    /// Serialized as a single ASCII byte (C*1) and converted to a <see cref="char"/>.
    /// </remarks>
    [C1] public char? TestType { get; set; }

    /// <summary>
    /// Test number. Optional.
    /// [STDF: TEST_NUM, U*4]
    /// </summary>
    public uint? TestNumber { get; set; }

    /// <summary>
    /// Number of times the test was executed. Optional.
    /// [STDF: EXEC_CNT, U*4]
    /// </summary>
    public uint? ExecutedCount { get; set; }

    /// <summary>
    /// Number of test failures. Optional.
    /// [STDF: FAIL_CNT, U*4]
    /// </summary>
    public uint? FailedCount { get; set; }

    /// <summary>
    /// Number of alarm conditions. Optional.
    /// [STDF: ALRM_CNT, U*4]
    /// </summary>
    public uint? AlarmCount { get; set; }

    /// <summary>
    /// Test name. Optional.
    /// [STDF: TEST_NAM, C*n]
    /// </summary>
    public string? TestName { get; set; }

    /// <summary>
    /// Sequencer (test program section) name. Optional.
    /// [STDF: SEQ_NAME, C*n]
    /// </summary>
    public string? SequencerName { get; set; }

    /// <summary>
    /// Test label or descriptive text. Optional.
    /// [STDF: TEST_LBL, C*n]
    /// </summary>
    public string? TestLabel { get; set; }

    /// <summary>
    /// Optional data flags indicating which summary statistics are valid. Bit-encoded. Optional.
    /// [STDF: OPT_FLAG, B*1]
    /// </summary>
    /// <remarks>
    /// Serialized as a bit field (B*1). Individual bits control the validity of
    /// <see cref="TestTimeAverage"/> (bit 2), <see cref="ResultMin"/> (bit 0),
    /// <see cref="ResultMax"/> (bit 1), <see cref="ResultSum"/> (bit 4),
    /// and <see cref="ResultSumOfSquares"/> (bit 5). A bit value of 0 means the corresponding field is valid.
    /// </remarks>
    [BitField] public byte? OptionalFlags { get; set; }

    /// <summary>
    /// Average test execution time in seconds. Optional. Valid only if <see cref="OptionalFlags"/> bit 2 is 0.
    /// [STDF: TEST_TIM, R*4]
    /// </summary>
    public float? TestTimeAverage { get; set; }

    /// <summary>
    /// Minimum test result value. Optional. Valid only if <see cref="OptionalFlags"/> bit 0 is 0.
    /// [STDF: TEST_MIN, R*4]
    /// </summary>
    public float? ResultMin { get; set; }

    /// <summary>
    /// Maximum test result value. Optional. Valid only if <see cref="OptionalFlags"/> bit 1 is 0.
    /// [STDF: TEST_MAX, R*4]
    /// </summary>
    public float? ResultMax { get; set; }

    /// <summary>
    /// Sum of all test result values. Optional. Valid only if <see cref="OptionalFlags"/> bit 4 is 0.
    /// [STDF: TST_SUMS, R*4]
    /// </summary>
    public float? ResultSum { get; set; }

    /// <summary>
    /// Sum of squares of test result values. Optional. Valid only if <see cref="OptionalFlags"/> bit 5 is 0.
    /// [STDF: TST_SQRS, R*4]
    /// </summary>
    public float? ResultSumOfSquares { get; set; }

    /// <summary>Gets <see cref="OptionalFlags"/> as a typed enum. See <see cref="OptionalFlags"/> for the raw value.</summary>
    public TsrOptionalFlags OptionalFlagsEnum => (TsrOptionalFlags)(OptionalFlags ?? 0);
}
