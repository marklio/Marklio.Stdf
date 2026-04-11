namespace Marklio.Stdf.Records;

/// <summary>
/// Optional data flags for <see cref="Tsr"/> (OPT_FLAG).
/// Indicates which summary statistic fields are valid.
/// A set bit means the corresponding field is <em>not</em> valid.
/// </summary>
[Flags]
public enum TsrOptionalFlags : byte
{
    /// <summary>No flags set — all statistics fields are valid.</summary>
    None = 0,

    /// <summary>TEST_MIN (result minimum) is invalid.</summary>
    ResultMinInvalid = 0x01,

    /// <summary>TEST_MAX (result maximum) is invalid.</summary>
    ResultMaxInvalid = 0x02,

    /// <summary>TEST_TIM (average test time) is invalid.</summary>
    TestTimeInvalid = 0x04,

    /// <summary>TST_SUMS (result sum) is invalid.</summary>
    ResultSumInvalid = 0x10,

    /// <summary>TST_SQRS (result sum of squares) is invalid.</summary>
    ResultSumOfSquaresInvalid = 0x20,
}
